/**
 * AppClient Phase 9 Integration Tests - App Network & Misc
 * 
 * Tests against REAL MixingStation API at http://localhost:8045
 * Phase 9: 3 endpoints (network/interfaces, network/interfaces/primary, save)
 */

using FluentAssertions;
using Xunit;

namespace MixingStation.Client.IntegrationTests.App
{
    [Trait("Category", "Integration")]
    [Trait("Phase", "9")]
    public class AppClientPhase9IntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetNetworkInterfacesAsync_ReturnsInterfaces()
        {
            // Arrange
            await RequireServerAsync();

            // Act
            var result = await AppClient.GetNetworkInterfacesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Interfaces.Should().NotBeNull();
            result.Interfaces.Should().NotBeEmpty("Server should have at least one network interface");

            // Verify first interface has expected properties
            var firstInterface = result.Interfaces[0];
            firstInterface.DisplayName.Should().NotBeNullOrEmpty();
            firstInterface.Name.Should().NotBeNullOrEmpty();
            firstInterface.IpAddress.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task PostNetworkInterfacesPrimaryAsync_SetsInterface()
        {
            // Arrange
            await RequireServerAsync();

            // Get available interfaces first
            var interfaces = await AppClient.GetNetworkInterfacesAsync();
            if (interfaces.Interfaces.Length == 0)
            {
                Assert.True(false, "SKIPPED: No network interfaces available");
            }

            var firstInterfaceName = interfaces.Interfaces[0].Name;
            var request = new MixingStation.Client.Models.NetworkInterfacePrimaryRequest
            {
                Name = firstInterfaceName
            };

            // Act
            var result = await AppClient.PostNetworkInterfacesPrimaryAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Interfaces.Should().NotBeNull();
            result.Interfaces.Should().Contain(i => i.Name == firstInterfaceName);
        }

        [Fact]
        public async Task PostSaveAsync_SavesSettings()
        {
            // Arrange
            await RequireServerAsync();

            // Act & Assert
            // Should not throw - saves current app settings
            await AppClient.PostSaveAsync();
        }
    }
}
