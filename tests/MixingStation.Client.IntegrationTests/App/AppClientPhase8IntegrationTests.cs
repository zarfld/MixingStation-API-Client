/**
 * AppClient Phase 8 Integration Tests - App IDCA & UI
 * 
 * Tests against REAL MixingStation API at http://localhost:8045
 * Phase 8: 6 endpoints (IDCA create/modify/delete/rearrange, UI selectedChannel)
 */

using FluentAssertions;
using Xunit;

namespace MixingStation.Client.IntegrationTests.App
{
    [Trait("Category", "Integration")]
    [Trait("Phase", "8")]
    public class AppClientPhase8IntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetUiSelectedChannelAsync_ReturnsChannel()
        {
            // Arrange
            await RequireServerAsync();

            // Act - May require mixer connected
            try
            {
                var result = await AppClient.GetUiSelectedChannelAsync();

                // Assert
                result.Should().NotBeNull();
                result.Index.Should().BeGreaterThanOrEqualTo(0);
            }
            catch (MixingStation.Client.Exceptions.TransportException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // Expected if no mixer connected
                Assert.True(false, "SKIPPED: No mixer connected - UI state not available");
            }
        }

        [Fact]
        public async Task PostIdcasAsync_Create_RequiresMixer()
        {
            // Arrange
            await RequireServerAsync();

            Assert.True(false, "SKIPPED: Skipped - IDCA operations require connected mixer");
        }

        [Fact]
        public async Task PostIdcasAsync_Modify_RequiresMixer()
        {
            // Arrange
            await RequireServerAsync();

            Assert.True(false, "SKIPPED: Skipped - IDCA operations require connected mixer");
        }

        [Fact]
        public async Task PostIdcasDeleteAsync_RequiresMixer()
        {
            // Arrange
            await RequireServerAsync();

            Assert.True(false, "SKIPPED: Skipped - IDCA operations require connected mixer");
        }

        [Fact]
        public async Task PostIdcasRearrangeAsync_RequiresMixer()
        {
            // Arrange
            await RequireServerAsync();

            Assert.True(false, "SKIPPED: Skipped - IDCA operations require connected mixer");
        }

        [Fact]
        public async Task GetUiSelectedChannelAsync_WithParam_RequiresMixer()
        {
            // Arrange
            await RequireServerAsync();

            Assert.True(false, "SKIPPED: Skipped - Setting channel requires connected mixer");
        }
    }
}
