/**
 * ConsoleClient Phase 10 Integration Tests - Console Config Events
 * 
 * Tests against REAL MixingStation API at http://localhost:8045
 * Phase 10: 1 endpoint (onConfigChanged - WebSocket event)
 */

using FluentAssertions;
using Xunit;

namespace MixingStation.Client.IntegrationTests.Console
{
    [Trait("Category", "Integration")]
    [Trait("Phase", "10")]
    public class ConsoleClientPhase10IntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetOnConfigChangedAsync_WebSocketEndpoint()
        {
            // Arrange
            await RequireServerAsync();

            // NOTE: This is a WebSocket-only endpoint
            // In REST mode it returns 204 No Content
            // Full testing would require WebSocket client

            // Act & Assert
            try
            {
                await ConsoleClient.GetOnConfigChangedAsync();
                // Should complete without error (204 No Content)
            }
            catch (MixingStation.Client.Exceptions.TransportException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Assert.True(false, "SKIPPED: WebSocket endpoint not available in REST mode");
            }
        }
    }
}
