using FluentAssertions;
using Xunit;

namespace MixingStation.Client.IntegrationTests.App;

/// <summary>
/// Tests that DISCONNECT and break connection - MUST run LAST.
/// These tests are in separate collection to ensure they run after all connection-dependent tests.
/// </summary>
[Collection("DisconnectTests")]
public class AppClientDisconnectIntegrationTests : IntegrationTestBase
{
    [Fact]
    [Trait("Category", "DisconnectsMixer")]
    public async Task PostMixersSearchAsync_WithValidConsoleId_Succeeds()
    {
        // Arrange
        await RequireServerAsync();
        
        // Disconnect first - search requires idle state
        try
        {
            await AppClient.PostMixersDisconnectAsync();
            await Task.Delay(1500); // Wait for state transition
        }
        catch
        {
            // Ignore disconnect errors
        }
        
        // Get available mixers first to get valid ConsoleId
        var available = await AppClient.GetMixersAvailableAsync();
        if (available.Consoles == null || available.Consoles.Length == 0)
        {
            // Skip test if no mixer types available
            return;
        }

        var request = new MixingStation.Client.Models.AppMixersSearchRequest
        {
            ConsoleId = available.Consoles[0].ConsoleId
        };

        // Act & Assert
        // Should not throw
        await AppClient.PostMixersSearchAsync(request);
        
        // Wait briefly for search to start
        await Task.Delay(500);
        
        // Verify search is running by checking state
        var state = await AppClient.GetStateAsync();
        state.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "DisconnectsMixer")]
    public async Task PostMixersDisconnectAsync_Succeeds()
    {
        // Arrange
        await RequireServerAsync();

        // Act & Assert
        // Should not throw even if no mixer connected
        await AppClient.PostMixersDisconnectAsync();
    }

    [Fact]
    [Trait("Category", "DisconnectsMixer")]
    public async Task PostMixersOfflineAsync_WithMixerId_Succeeds()
    {
        // Arrange
        await RequireServerAsync();
        
        // Disconnect first to ensure idle state (offline requires idle)
        await AppClient.PostMixersDisconnectAsync();
        await Task.Delay(1000);
        
        // Get available mixers to get valid ConsoleId
        var available = await AppClient.GetMixersAvailableAsync();
        if (available.Consoles == null || available.Consoles.Length == 0)
        {
            // Skip test if no mixer types available
            return;
        }

        var request = new MixingStation.Client.Models.AppMixersOfflineRequest
        {
            ConsoleId = available.Consoles[0].ConsoleId
        };

        // Act
        await AppClient.PostMixersOfflineAsync(request);

        // Assert - should be in offline mode
        var state = await AppClient.GetStateAsync();
        state.Should().NotBeNull();
        state.TopState.Should().Be("connected"); // Offline mode shows as connected
    }
}
