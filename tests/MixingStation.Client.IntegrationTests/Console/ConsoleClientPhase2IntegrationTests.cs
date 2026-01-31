/**
 * ConsoleClient Phase 2 Integration Tests - Console Data Subscription
 * 
 * Tests against REAL MixingStation API at http://localhost:8045
 * Phase 2: 3 endpoints (data/subscribe, data/set, information)
 */

using FluentAssertions;
using Xunit;

namespace MixingStation.Client.IntegrationTests.Console
{
    [Trait("Category", "Integration")]
    [Trait("Phase", "2")]
    public class ConsoleClientPhase2IntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetInformationAsync_RequiresMixer()
        {
            // Arrange
            await RequireServerAsync();

            // Act & Assert
            try
            {
                var result = await ConsoleClient.GetInformationAsync();

                result.Should().NotBeNull();
                result.TotalChannels.Should().BeGreaterThan(0);
                result.ChannelColors.Should().NotBeNull();
                result.ChannelTypes.Should().NotBeNull();
            }
            catch (MixingStation.Client.Exceptions.TransportException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Assert.True(false, "SKIPPED: No mixer connected - console information not available");
            }
        }

        [Fact]
        public async Task PostDataSubscribeAsync_RequiresMixer()
        {
            // Arrange
            await RequireServerAsync();

            Assert.True(false, "SKIPPED: Skipped - data subscription requires connected mixer");
        }

        [Fact]
        public async Task PostDataSetAsync_RequiresMixer()
        {
            // Arrange
            await RequireServerAsync();

            Assert.True(false, "SKIPPED: Skipped - setting data requires connected mixer");
        }
    }
}
