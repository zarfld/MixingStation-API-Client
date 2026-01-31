/**
 * AppClient - HTTP transport implementation for /app/* endpoints
 * 
 * SOURCE OF TRUTH: docs/openapi.json (http://localhost:8045/openapi.json)
 * Public API mirrors REST endpoints exactly (ADR-002: #11)
 * 
 * Implements:
 * - #4 REQ-F-001: HTTP Client Transport
 * - #5 REQ-F-002: Application State Reading
 * 
 * Architecture:
 * - #10 ADR-001: .NET 8 Runtime
 * - #11 ADR-002: HTTP Transport + REST Mirror Naming Policy
 * 
 * Design: Phase 1 Detailed Design (04-design/phase-1-detailed-design.md)
 * Reference: docs/openapi-spec-usage.md
 * 
 * TDD Status: GREEN Phase - Implementation complete
 * 
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/4
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/5
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/11
 */

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MixingStation.Client.Exceptions;
using MixingStation.Client.Models;

namespace MixingStation.Client.App
{
    /// <summary>
    /// HTTP transport implementation for /app/* endpoints.
    /// TDD GREEN Phase: Full implementation with error handling.
    /// </summary>
    public class AppClient : IAppClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        /// <summary>
        /// JSON serialization options for API requests/responses.
        /// Uses camelCase property naming to match REST API.
        /// </summary>
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public AppClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <inheritdoc />
        public async Task PostMixersConnectAsync(
            AppMixersConnectRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.PostAsJsonAsync(
                    "/app/mixers/connect",
                    request,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/mixers/connect",
                        response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new TransportException($"Request timeout: {ex.Message}", null, ex);
            }
            catch (TransportException)
            {
                // Re-throw TransportException as-is
                throw;
            }
            catch (Exception ex)
            {
                throw new TransportException($"Unexpected error: {ex.Message}", null, ex);
            }
        }

        /// <inheritdoc />
        public async Task<AppMixersCurrentResponse> GetMixersCurrentAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(
                    "/app/mixers/current",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from GET /app/mixers/current",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AppMixersCurrentResponse>(
                    cancellationToken: cancellationToken);

                if (result == null)
                {
                    throw new TransportException("Failed to deserialize response: null result");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new TransportException($"Request timeout: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"JSON deserialization error: {ex.Message}", null, ex);
            }
            catch (TransportException)
            {
                // Re-throw TransportException as-is
                throw;
            }
            catch (Exception ex)
            {
                throw new TransportException($"Unexpected error: {ex.Message}", null, ex);
            }
        }

        /// <inheritdoc />
        public async Task<AppStateResponse> GetStateAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                // GET /app/state
                var response = await httpClient.GetAsync("/app/state", cancellationToken)
                    .ConfigureAwait(false);

                // Check for HTTP errors
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken)
                        .ConfigureAwait(false);
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode}: {errorContent}",
                        response.StatusCode);
                }

                // Deserialize response
                var result = await response.Content.ReadFromJsonAsync<AppStateResponse>(
                    JsonOptions, cancellationToken).ConfigureAwait(false);

                if (result == null)
                {
                    throw new TransportException("Failed to deserialize response: received null");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"Failed to deserialize response: {ex.Message}", null, ex);
            }
            catch (TransportException)
            {
                // Re-throw TransportException as-is
                throw;
            }
            catch (OperationCanceledException)
            {
                // Re-throw cancellation as-is
                throw;
            }
            catch (Exception ex)
            {
                throw new TransportException($"Unexpected error: {ex.Message}", null, ex);
            }
        }
    }
}
