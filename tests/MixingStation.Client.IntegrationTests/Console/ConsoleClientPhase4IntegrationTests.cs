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
            // Arrange - Connect to real mixer
            await RequireMixerAsync();

            // Get available paths first - root paths are in Child dictionary
            var paths = await ConsoleClient.GetDataPathsAsync();
            if (paths.Child == null || paths.Child.Count == 0)
            {
                Assert.Fail("No data paths available from mixer");
            }

            // Find a path with actual values (deprecated API requires full value path)
            // Navigate: root -> "ch" -> "0" -> values array -> "solo"
            string testPath;
            if (paths.Child.TryGetValue("ch", out var chPath) &&
                chPath.Child != null && chPath.Child.Count > 0)
            {
                var firstChannel = chPath.Child.First().Value;
                if (firstChannel.Val != null && firstChannel.Val.Length > 0)
                {
                    var channelIndex = chPath.Child.First().Key;
                    var valueName = firstChannel.Val[0];
                    testPath = $"ch.{channelIndex}.{valueName}";
                }
                else
                {
                    Assert.Fail("No values found in channel paths");
                    return;
                }
            }
            else
            {
                Assert.Fail("Channel paths not available");
                return;
            }

            // Act
            #pragma warning disable CS0618 // Obsolete
            var result = await ConsoleClient.GetDataDefinitionsAsync(testPath);
            #pragma warning restore CS0618

            // Assert
            result.Should().NotBeNull();
            result.Definitions.Should().NotBeNull();
            result.Definitions.Should().ContainKey(testPath);
        }

        [Fact]
        public async Task GetDataDefinitions2Async_RequiresMixer()
        {
            // Arrange - Connect to real mixer
            await RequireMixerAsync();

            // Get available paths first - root paths are in Child dictionary
            var paths = await ConsoleClient.GetDataPathsAsync();
            if (paths.Child == null || paths.Child.Count == 0)
            {
                Assert.Fail("No data paths available from mixer");
            }

            // Use first available root path (e.g., "ch", "fx", "muteGroups")
            var testPath = paths.Child.Keys.First();

            // Act
            var result = await ConsoleClient.GetDataDefinitions2Async(testPath);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact(Skip = "Data subscription requires WebSocket connection (not available in REST mode)")]


        public async Task PostDataUnsubscribeAsync_RequiresMixer()
        {
            // Arrange
            await RequireMixerAsync();

            // SKIP: This endpoint requires WebSocket connection, not available in REST mode
            // Test skipped - requires physical mixer
            
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
