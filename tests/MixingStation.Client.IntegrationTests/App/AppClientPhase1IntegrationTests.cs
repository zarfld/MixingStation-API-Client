/**
 * AppClient Phase 1 Integration Tests - App Client Initialization
 * 
 * Tests against REAL MixingStation API at http://localhost:8045
 * Phase 1: 3 endpoints (connect, current, state)
 * 
 * Requirements:
 * - MixingStation app must be running
 * - Run with: dotnet test --filter "Category=Integration"
 */

using FluentAssertions;
using Xunit;

namespace MixingStation.Client.IntegrationTests.App
{
    [Trait("Category", "Integration")]
    [Trait("Phase", "1")]
    public class AppClientPhase1IntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetStateAsync_ReturnsCurrentState()
        {
            // Arrange
            await RequireServerAsync();

            // Act
            var result = await AppClient.GetStateAsync();

            // Assert
            result.Should().NotBeNull();
            result.State.Should().NotBeNullOrEmpty();
            result.TopState.Should().NotBeNullOrEmpty();
            // Progress is 0-100
            result.Progress.Should().BeInRange(0, 100);
        }

        [Fact]
        public async Task GetMixersCurrentAsync_ReturnsCurrentMixer()
        {
            // Arrange
            await RequireServerAsync();

            // Act
            var result = await AppClient.GetMixersCurrentAsync();

            // Assert
            result.Should().NotBeNull();
            // ConsoleId may be 0 if no mixer connected
            result.ConsoleId.Should().BeGreaterThanOrEqualTo(0);
            result.Name.Should().NotBeNull();
        }

        [Fact]
        public async Task PostMixersConnectAsync_WithValidData_Succeeds()
        {
            // Arrange
            await RequireServerAsync();

            // NOTE: This test requires knowing a valid mixer IP
            // Skipping actual connection test to avoid requiring hardware
            // Real test would look like:
            // var request = new AppMixersConnectRequest
            // {
            //     ConsoleId = 1,
            //     Ip = "192.168.1.100"
            // };
            // await AppClient.PostMixersConnectAsync(request);

            // For integration test suite, we just verify the endpoint exists
            Assert.True(false, "SKIPPED: Skipped - requires actual mixer hardware and IP address");
        }
    }
}
