/**
 * ConsoleClient Phase 6 Integration Tests - Console Metering
 * 
 * Tests against REAL MixingStation API at http://localhost:8045
 * Phase 6: 3 endpoints (metering/subscribe, metering/unsubscribe, metering2/subscribe)
 */

using FluentAssertions;
using Xunit;

namespace MixingStation.Client.IntegrationTests.Console
{
    [Trait("Category", "Integration")]
    [Trait("Phase", "6")]
    public class ConsoleClientPhase6IntegrationTests : IntegrationTestBase
    {
        [Fact(Skip = "Metering subscription requires WebSocket for data delivery (not available in REST-only mode)")]
        public async Task PostMeteringSubscribeAsync_RequiresMixer()
        {
            await Task.CompletedTask;
        }

        [Fact(Skip = "Metering unsubscription requires WebSocket (not available in REST-only mode)")]
        public async Task PostMeteringUnsubscribeAsync_RequiresMixer()
        {
            await Task.CompletedTask;
        }

        [Fact(Skip = "Metering2 subscription requires WebSocket for data delivery (not available in REST-only mode)")]
        public async Task PostMetering2SubscribeAsync_RequiresMixer()
        {
            await Task.CompletedTask;
        }
    }
}
