/**
 * AppClient Phase 3 Integration Tests - App Mixer Lifecycle
 * 
 * Tests against REAL MixingStation API at http://localhost:8045
 * Phase 3: 5 endpoints (available, search, searchResults, disconnect, offline)
 */

using FluentAssertions;
using Xunit;

namespace MixingStation.Client.IntegrationTests.App
{
    [Trait("Category", "Integration")]
    [Trait("Phase", "3")]
    public class AppClientPhase3IntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetMixersAvailableAsync_ReturnsAvailableMixers()
        {
            // Arrange
            await RequireServerAsync();

            // Act
            var result = await AppClient.GetMixersAvailableAsync();

            // Assert
            result.Should().NotBeNull();
            result.Consoles.Should().NotBeNull();
            // May be empty if no mixers available, but should not be null
        }

        [Fact]
        public async Task PostMixersSearchAsync_StartsSearch()
        {
            // Arrange
            await RequireServerAsync();
            
            // Get available mixers first to get valid ConsoleId
            var available = await AppClient.GetMixersAvailableAsync();
            if (available.Consoles == null || available.Consoles.Length == 0)
            {
                Assert.True(false, "SKIPPED: No mixer types available for search");
            }

            var request = new MixingStation.Client.Models.AppMixersSearchRequest
            {
                ConsoleId = available.Consoles[0].ConsoleId
            };

            // Act & Assert
            // Should not throw - starts background search
            await AppClient.PostMixersSearchAsync(request);
        }

        [Fact]
        public async Task GetMixersSearchResultsAsync_ReturnsResults()
        {
            // Arrange
            await RequireServerAsync();

            // Act
            var result = await AppClient.GetMixersSearchResultsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Results.Should().NotBeNull();
        }

        [Fact]
        public async Task PostMixersDisconnectAsync_Succeeds()
        {
            // Arrange
            await RequireServerAsync();

            // Act & Assert
            // Should not throw even if no mixer connected
            await AppClient.PostMixersDisconnectAsync();
        }

        [Fact]
        public async Task PostMixersOfflineAsync_WithMixerId_Succeeds()
        {
            // Arrange
            await RequireServerAsync();
            
            // Get available mixers first to get valid ConsoleId and ModelId
            var available = await AppClient.GetMixersAvailableAsync();
            if (available.Consoles == null || available.Consoles.Length == 0)
            {
                Assert.True(false, "SKIPPED: No mixer types available for offline mode");
            }

            var console = available.Consoles[0];
            var request = new MixingStation.Client.Models.AppMixersOfflineRequest
            {
                ConsoleId = console.ConsoleId,
                ModelId = console.Models?.Length > 0 ? 0 : 0, // Use first model if available
                Model = console.Models?.Length > 0 ? console.Models[0] : ""
            };

            // Act & Assert
            await AppClient.PostMixersOfflineAsync(request);
        }
    }
}
