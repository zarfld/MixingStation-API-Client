/**
 * ConsoleClient Phase 10 Tests - Console Config Events
 * 
 * Phase 10 Coverage: Console Config Events (1 endpoint - FINAL PHASE!)
 * 1. GET /console/onConfigChanged (WebSocket event endpoint)
 * 
 * Test Strategy: TDD Red-Green-Refactor
 * - Success: Mock HttpMessageHandler, return 204 No Content
 * - HTTP Error: Return error status, assert StatusCode == HttpStatusCode.X
 * - Network Error: Throw HttpRequestException, assert TransportException.InnerException type
 * 
 * Pattern: Endpoint has 3 tests (Success, HTTP Error, Network Error)
 * Expected: 3 tests total (1 endpoint × 3 tests)
 * 
 * Note: This endpoint is WebSocket-only in production (broadcasts config change events).
 * In REST context, it returns 204 No Content. Full implementation would require WebSocket handling.
 */

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using MixingStation.Client.Console;
using MixingStation.Client.Exceptions;
using Xunit;

namespace MixingStation.Client.Tests.Console
{
    public class ConsoleClientPhase10Tests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly IConsoleClient _consoleClient;
        private readonly string _baseUrl = "http://localhost:8045";

        public ConsoleClientPhase10Tests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri(_baseUrl)
            };

            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory>(sp =>
            {
                var factoryMock = new Mock<IHttpClientFactory>();
                factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
                return factoryMock.Object;
            });

            var serviceProvider = services.BuildServiceProvider();
            _consoleClient = new ConsoleClient(serviceProvider.GetRequiredService<IHttpClientFactory>());
        }

        // ========================================
        // GET /console/onConfigChanged Tests
        // ========================================

        [Fact]
        public async Task GetOnConfigChangedAsync_Success_ReturnsVoid()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/console/onConfigChanged"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            // Act
            Func<Task> act = async () => await _consoleClient.GetOnConfigChangedAsync();

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetOnConfigChangedAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _consoleClient.GetOnConfigChangedAsync());

            exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetOnConfigChangedAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network failure"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _consoleClient.GetOnConfigChangedAsync());

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }
    }
}
