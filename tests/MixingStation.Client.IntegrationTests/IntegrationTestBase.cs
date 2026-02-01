/**
 * Integration Test Base - Common infrastructure for integration tests
 * 
 * Tests REAL HTTP calls against MixingStation API at http://localhost:8045
 * No mocking - actual server must be running
 * 
 * Requirements:
 * - MixingStation app must be running on localhost:8045
 * - Tests are categorized as [Trait("Category", "Integration")]
 * - Run with: dotnet test --filter "Category=Integration"
 * - Skip with: dotnet test --filter "Category!=Integration"
 */

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using MixingStation.Client.App;
using MixingStation.Client.Console;
using Xunit;

namespace MixingStation.Client.IntegrationTests
{
    /// <summary>
    /// Base class for integration tests with real HTTP client
    /// </summary>
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly IAppClient AppClient;
        protected readonly IConsoleClient ConsoleClient;
        protected readonly HttpClient HttpClient;
        protected const string BaseUrl = "http://127.0.0.1:8045"; // Use IPv4 explicitly (localhost may resolve to IPv6)

        protected IntegrationTestBase()
        {
            // Create real HTTP client (no mocking)
            HttpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Setup DI container with real HTTP client factory
            var services = new ServiceCollection();
            services.AddHttpClient();

            // Override factory to use our test HttpClient
            services.AddSingleton<IHttpClientFactory>(sp =>
            {
                return new TestHttpClientFactory(HttpClient);
            });

            var serviceProvider = services.BuildServiceProvider();

            // Create real clients
            AppClient = new AppClient(serviceProvider.GetRequiredService<IHttpClientFactory>());
            ConsoleClient = new ConsoleClient(serviceProvider.GetRequiredService<IHttpClientFactory>());
        }

        /// <summary>
        /// Check if MixingStation server is running
        /// </summary>
        protected async Task<bool> IsServerAvailableAsync()
        {
            try
            {
                var response = await HttpClient.GetAsync("/app/state");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if mixer is connected
        /// </summary>
        protected async Task<bool> IsMixerConnectedAsync()
        {
            try
            {
                var state = await AppClient.GetStateAsync();
                System.Console.WriteLine($"[DEBUG] State: {state.State}, TopState: {state.TopState}");
                return state.TopState == "connected";
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[DEBUG] IsMixerConnectedAsync exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Skip test if server is not available
        /// </summary>
        protected async Task RequireServerAsync()
        {
            if (!await IsServerAvailableAsync())
            {
                Assert.Fail("FAIL: MixingStation server is not running at http://127.0.0.1:8045");
            }
        }

        /// <summary>
        /// Ensure mixer is connected - connects automatically if needed
        /// </summary>
        protected async Task RequireMixerAsync()
        {
            await RequireServerAsync();
            
            // Check if already connected
            if (await IsMixerConnectedAsync())
            {
                System.Console.WriteLine("[DEBUG] Mixer already connected");
                return;
            }

            System.Console.WriteLine("[DEBUG] Mixer not connected - discovering mixers on network...");

            // Check current state - REUSE existing connection if available
            var currentState = await AppClient.GetStateAsync();
            
            // If already connected, reuse it (don't disconnect - preserves manual firmware dialog approval)
            if (currentState.TopState == "connected")
            {
                try
                {
                    var current = await AppClient.GetMixersCurrentAsync();
                    if (!string.IsNullOrEmpty(current?.CurrentModel))
                    {
                        System.Console.WriteLine($"[DEBUG] ✅ Mixer already connected: {current.CurrentModel} at {current.IpAddress}");
                        System.Console.WriteLine($"[DEBUG] Reusing existing connection (preserving manual firmware approval)");
                        return;
                    }
                }
                catch
                {
                    System.Console.WriteLine("[DEBUG] State='connected' but /current failed - will reconnect");
                }
            }
            
            // Only disconnect if in connecting/error state (not idle, not connected)
            if (currentState.TopState != "idle" && currentState.TopState != "connected")
            {
                System.Console.WriteLine($"[DEBUG] State is '{currentState.TopState}' - disconnecting to reset...");
                await AppClient.PostMixersDisconnectAsync();
                await Task.Delay(2000); // Wait for disconnect to complete
                
                // Verify we're now idle
                currentState = await AppClient.GetStateAsync();
                if (currentState.TopState != "idle")
                {
                    Assert.Fail($"FAIL: Could not reset to idle state (still {currentState.TopState})");
                }
                System.Console.WriteLine("[DEBUG] Successfully reset to idle state.");
            }

            // Get available mixer types
            var availableMixers = await AppClient.GetMixersAvailableAsync();
            if (availableMixers?.Consoles == null || availableMixers.Consoles.Length == 0)
            {
                Assert.Fail("FAIL: No mixer types available.");
            }

            // Try each mixer type until we find one on the network
            // Try network discovery first (fast scan - 2 seconds per type)
            foreach (var mixerType in availableMixers.Consoles)
            {
                System.Console.WriteLine($"[DEBUG] Searching for {mixerType.Name} mixers on network...");
                
                // Start network search
                await AppClient.PostMixersSearchAsync(new MixingStation.Client.Models.AppMixersSearchRequest
                {
                    ConsoleId = mixerType.ConsoleId
                });

                // Wait for search to discover devices (reduced to 2 seconds for faster test execution)
                await Task.Delay(2000);

                // Get search results (search might still be running in background)
                var searchResults = await AppClient.GetMixersSearchResultsAsync();
                
                if (searchResults?.Results != null && searchResults.Results.Length > 0)
                {
                    System.Console.WriteLine($"[DEBUG] Found {searchResults.Results.Length} {mixerType.Name} mixer(s)");
                    
                    // Prioritize by mixer model (stable identifier, not IP which can change via DHCP)
                    // Prefer "SC" models (like 32SC) over "R" models (like 32R) as they're more reliable
                    var prioritizedMixers = searchResults.Results
                        .OrderByDescending(m => m.Model?.Contains("SC") == true ? 1 : 0)
                        .ThenBy(m => m.Model)
                        .ToArray();
                    
                    // Try each discovered mixer until one connects successfully
                    foreach (var discoveredMixer in prioritizedMixers)
                    {
                        System.Console.WriteLine($"[DEBUG] Trying {discoveredMixer.Name} at {discoveredMixer.Ip}...");

                        // CRITICAL: Disconnect first to clear any orphaned connections
                        // NOTE: This only runs if we're NOT already connected (early return above prevents this)
                        System.Console.WriteLine($"[DEBUG] Disconnecting to clear orphaned connections...");
                        await AppClient.PostMixersDisconnectAsync();
                        await Task.Delay(1000); // Wait for clean disconnect

                        // Connect to discovered mixer (this will stop the search)
                        var connectRequest = new MixingStation.Client.Models.AppMixersConnectRequest
                        {
                            ConsoleId = mixerType.ConsoleId,
                            Ip = discoveredMixer.Ip
                        };

                        System.Console.WriteLine($"[DEBUG] Connecting to {discoveredMixer.Name}...");
                        await AppClient.PostMixersConnectAsync(connectRequest);

                        // Give the connection more time to establish before polling
                        // Manual testing shows connection succeeds but takes 5+ seconds
                        System.Console.WriteLine($"[DEBUG] Waiting 5 seconds for connection to establish...");
                        await Task.Delay(5000);

                        // Check connection via /app/mixers/current (more reliable than state polling)
                        // Manual tests show this works even when state is stuck on "connecting"
                        bool connected = false;
                        for (int i = 0; i < 10; i++)
                        {
                            try
                            {
                                var current = await AppClient.GetMixersCurrentAsync();
                                
                                // Handle null response or empty data
                                if (current == null)
                                {
                                    System.Console.WriteLine($"[DEBUG] Attempt {i + 1}/10 - /current returned null");
                                }
                                else
                                {
                                    System.Console.WriteLine($"[DEBUG] Attempt {i + 1}/10 - /current: ConsoleId={current.ConsoleId}, Model='{current.CurrentModel ?? "(null)"}', Manufacturer='{current.Manufacturer ?? "(null)"}', IpAddress='{current.IpAddress ?? "(null)"}'");
                                    
                                    // Check if we got real mixer data matching the one we're trying to connect to
                                    if (!string.IsNullOrEmpty(current.CurrentModel) && 
                                        !string.IsNullOrEmpty(current.Manufacturer) &&
                                    current.ConsoleId > 0)
                                {
                                    System.Console.WriteLine($"[DEBUG] ✅ Successfully connected to {current.Name} ({current.CurrentModel}) at {current.IpAddress}!");
                                    connected = true;
                                    return; // Success!
                                    }
                                    
                                    System.Console.WriteLine($"[DEBUG] /current returned empty data - waiting...");
                                }
                            }
                            catch (System.Text.Json.JsonException jsonEx)
                            {
                                // Empty JSON response (content-length: 0) - connection still establishing
                                System.Console.WriteLine($"[DEBUG] Attempt {i + 1}/10 - Empty JSON response (connection establishing...)");
                            }
                            catch (Exception ex)
                            {
                                System.Console.WriteLine($"[DEBUG] /current failed: {ex.Message}");
                            }
                            
                            // Check state for explicit failure
                            try
                            {
                                var state = await AppClient.GetStateAsync();
                                if (state.TopState == "idle" || state.TopState == "error")
                                {
                                    System.Console.WriteLine($"[DEBUG] ❌ Connection failed (state: {state.TopState}, message: {state.Msg})");
                                    connected = false;
                                    break;
                                }
                            }
                            catch { }
                            
                            await Task.Delay(2000); // Wait 2 seconds between checks (less aggressive polling)
                        }
                        
                        if (!connected)
                        {
                            // Connection timed out - disconnect and try next
                            System.Console.WriteLine($"[DEBUG] Connection timeout for {discoveredMixer.Name} - trying next mixer...");
                            await AppClient.PostMixersDisconnectAsync();
                            await Task.Delay(2000); // Wait for disconnect
                        }
                    }
                }
                else
                {
                    // No mixers found for this type - disconnect to stop search before trying next type
                    System.Console.WriteLine($"[DEBUG] No {mixerType.Name} mixers found - stopping search...");
                    await AppClient.PostMixersDisconnectAsync();
                    await Task.Delay(1000); // Let disconnect complete
                }
            }

            // No physical mixer found - try OFFLINE mode as fallback
            System.Console.WriteLine("[DEBUG] No physical mixers found - attempting OFFLINE mode...");
            
            // Use StudioLive 32SC in offline mode (most common/reliable model)
            var offlineRequest = new MixingStation.Client.Models.AppMixersOfflineRequest
            {
                ConsoleId = 18, // StudioLive III
                ModelId = 12,   // StudioLive 32SC
                Model = "StudioLive 32SC"
            };
            
            await AppClient.PostMixersOfflineAsync(offlineRequest);
            await Task.Delay(2000); // Wait for offline mode to initialize
            
            // Verify offline mode is active
            var finalState = await AppClient.GetStateAsync();
            if (finalState.TopState == "connected")
            {
                System.Console.WriteLine("[DEBUG] Successfully started OFFLINE mode (StudioLive 32SC)");
                return;
            }
            
            Assert.Fail("FAIL: Could not connect to any mixer (tried network discovery + offline mode).");
        }

        public void Dispose()
        {
            HttpClient?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Test HTTP client factory that returns our test HttpClient
        /// </summary>
        private class TestHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpClient _httpClient;

            public TestHttpClientFactory(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public HttpClient CreateClient(string name)
            {
                return _httpClient;
            }
        }
    }
}
