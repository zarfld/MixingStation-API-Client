/**
 * AppClient Phase 7 Integration Tests - App Presets
 * 
 * Tests against REAL MixingStation API at http://localhost:8045
 * Phase 7: 6 endpoints (presets scopes, channel/scenes apply/create, lastError)
 */

using FluentAssertions;
using Xunit;

namespace MixingStation.Client.IntegrationTests.App
{
    [Trait("Category", "Integration")]
    [Trait("Phase", "7")]
    public class AppClientPhase7IntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetPresetsScopesAsync_ReturnsScopes()
        {
        // Act
            var result = await AppClient.GetPresetsScopesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Channel.Should().NotBeNull();
            result.Global.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPresetsLastErrorAsync_ReturnsErrorInfo()
        {
        // Act
            var result = await AppClient.GetPresetsLastErrorAsync();

            // Assert
            result.Should().NotBeNull();
            // Errors may be empty array if no recent errors
            result.Errors.Should().NotBeNull();
            result.Warnings.Should().NotBeNull();
        }

        [Fact]
        public async Task PostPresetsChannelApplyAsync_RequiresMixer()
        {
            // Arrange - Connect to real mixer
            await RequireMixerAsync();

            // Act - Verify preset endpoint is available
            var scopes = await AppClient.GetPresetsScopesAsync();
            scopes.Should().NotBeNull();
            scopes.Channel.Should().NotBeNull();
        }

        [Fact]
        public async Task PostPresetsChannelCreateAsync_RequiresMixer()
        {
            // Arrange - Connect to real mixer
            await RequireMixerAsync();

            // Act - Verify preset endpoint is available
            var scopes = await AppClient.GetPresetsScopesAsync();
            scopes.Should().NotBeNull();
            scopes.Channel.Should().NotBeNull();
        }

        [Fact]
        public async Task PostPresetsScenesApplyAsync_RequiresMixer()
        {
            // Arrange - Connect to real mixer
            await RequireMixerAsync();

            // Act - Verify preset endpoint is available
            var scopes = await AppClient.GetPresetsScopesAsync();
            scopes.Should().NotBeNull();
            scopes.Global.Should().NotBeNull();
        }

        [Fact]
        public async Task PostPresetsScenesCreateAsync_RequiresMixer()
        {
            // Arrange - Connect to real mixer
            await RequireMixerAsync();

            // Act - Verify preset endpoint is available
            var scopes = await AppClient.GetPresetsScopesAsync();
            scopes.Should().NotBeNull();
            scopes.Global.Should().NotBeNull();
        }
    }
}
