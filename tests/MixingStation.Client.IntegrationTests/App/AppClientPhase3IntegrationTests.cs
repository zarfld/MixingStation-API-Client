/**
 * AppClient Phase 3 Integration Tests - App Mixer Lifecycle
 * 
 * Tests against REAL MixingStation API at http://localhost:8045
 * Phase 3: 5 endpoints (available, search, searchResults, disconnect, offline)
 */

using FluentAssertions;
using MixingStation.Client.Exceptions;
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
        
        // NOTE: Disconnect/Search/Offline tests moved to AppClientDisconnectIntegrationTests.cs
        // to ensure they run LAST and don't break connection-dependent tests
    }
}
