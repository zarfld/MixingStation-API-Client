/**
 * ConsoleClient Phase 4 Integration Tests - Console Data Discovery
 * 
 * Tests against REAL MixingStation API at http://localhost:8045
 * Phase 4: 6 endpoints (categories, paths, definitions, unsubscribe)
 */

using FluentAssertions;
using Xunit;

namespace MixingStation.Client.IntegrationTests.Console
{
    [Trait("Category", "Integration")]
    [Trait("Phase", "4")]
    public class ConsoleClientPhase4IntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetDataCategoriesAsync_RequiresMixer()
        {
            // Arrange
            await RequireMixerAsync(); // Check mixer connected first

            // Act
            var result = await ConsoleClient.GetDataCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Categories.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDataPathsAsync_RequiresMixer()
        {
            // Arrange
            await RequireMixerAsync();

            // Act
            var result = await ConsoleClient.GetDataPathsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Val.Should().NotBeNull();
            result.Child.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDataPathsByPathAsync_RequiresMixer()
        {
            // Arrange
            await RequireMixerAsync();

            // Act - Query specific path (e.g., channel data)
            var result = await ConsoleClient.GetDataPathsAsync("ch");

            // Assert
            result.Should().NotBeNull();
            result.Val.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDataDefinitionsAsync_RequiresMixer()
        {
            // Arrange
            await RequireMixerAsync();

            // Get available paths first
            var paths = await ConsoleClient.GetDataPathsAsync();
            if (paths.Val == null || paths.Val.Length == 0)
            {
                Assert.True(false, "SKIPPED: No data paths available");
            }

            var testPath = paths.Val[0];

            // Act
            #pragma warning disable CS0618 // Obsolete
            var result = await ConsoleClient.GetDataDefinitionsAsync(testPath);
            #pragma warning restore CS0618

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDataDefinitions2Async_RequiresMixer()
        {
            // Arrange
            await RequireMixerAsync();

            // Get available paths first
            var paths = await ConsoleClient.GetDataPathsAsync();
            if (paths.Val == null || paths.Val.Length == 0)
            {
                Assert.True(false, "SKIPPED: No data paths available");
            }

            var testPath = paths.Val[0];

            // Act
            var result = await ConsoleClient.GetDataDefinitions2Async(testPath);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task PostDataUnsubscribeAsync_RequiresMixer()
        {
            // Arrange
            await RequireMixerAsync();

            // Subscribe first so we have something to unsubscribe
            var subscribeReq = new MixingStation.Client.Models.ConsoleDataSubscribeRequest
            {
                Path = "ch.0.mix.lvl",
                Format = "val"
            };
            await ConsoleClient.PostDataSubscribeAsync(subscribeReq);

            var request = new MixingStation.Client.Models.ConsoleDataSubscribeRequest
            {
                Path = "ch.0.mix.lvl",
                Format = "val"
            };

            // Act & Assert
            await ConsoleClient.PostDataUnsubscribeAsync(request);
        }
    }
}
