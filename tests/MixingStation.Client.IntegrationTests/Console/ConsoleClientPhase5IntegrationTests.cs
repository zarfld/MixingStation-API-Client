/**
 * ConsoleClient Phase 5 Integration Tests - Console Auth & Mix Targets
 * 
 * Tests against REAL MixingStation API at http://localhost:8045
 * Phase 5: 3 endpoints (auth/info, auth/login, mixTargets)
 */

using FluentAssertions;
using Xunit;

namespace MixingStation.Client.IntegrationTests.Console
{
    [Trait("Category", "Integration")]
    [Trait("Phase", "5")]
    public class ConsoleClientPhase5IntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetAuthInfoAsync_RequiresMixer()
        {
            // Arrange
            await RequireServerAsync();

            try
            {
                var result = await ConsoleClient.GetAuthInfoAsync();

                result.Should().NotBeNull();
            }
            catch (MixingStation.Client.Exceptions.TransportException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Assert.True(false, "SKIPPED: No mixer connected");
            }
        }

        [Fact]
        public async Task PostAuthLoginAsync_RequiresMixer()
        {
            // Arrange
            await RequireServerAsync();

            Assert.True(false, "SKIPPED: Skipped - login requires connected mixer and valid credentials");
        }

        [Fact]
        public async Task GetMixTargetsAsync_RequiresMixer()
        {
            // Arrange
            await RequireServerAsync();

            try
            {
                var result = await ConsoleClient.GetMixTargetsAsync();

                result.Should().NotBeNull();
                result.Targets.Should().NotBeNull();
            }
            catch (MixingStation.Client.Exceptions.TransportException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Assert.True(false, "SKIPPED: No mixer connected");
            }
        }
    }
}
