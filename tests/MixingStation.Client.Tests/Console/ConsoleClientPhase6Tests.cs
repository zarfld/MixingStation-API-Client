/**
 * ConsoleClientPhase6Tests - TDD test suite for Phase 6 endpoints
 * 
 * Phase 6: Console Metering (3 endpoints)
 * - POST /console/metering/subscribe [DEPRECATED]
 * - POST /console/metering/unsubscribe
 * - POST /console/metering2/subscribe
 * 
 * Test Pattern: 3 tests per endpoint (success, HTTP error, network error)
 * Expected: 9 tests total (3 endpoints × 3 tests)
 * 
 * See: .github/copilot-instructions.md (TDD Red-Green-Refactor)
 */

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using MixingStation.Client.App;
using MixingStation.Client.Console;
using MixingStation.Client.Exceptions;
using MixingStation.Client.Models;
using Xunit;

namespace MixingStation.Client.Tests.Console
{
    public class ConsoleClientPhase6Tests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly ConsoleClient _sut;

        public ConsoleClientPhase6Tests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            _sut = new ConsoleClient(_httpClientFactoryMock.Object);
        }

        // ========================================
        // POST /console/metering/subscribe [DEPRECATED]
        // ========================================

        [Fact]
        public async Task PostMeteringSubscribeAsync_Success_ReturnsVoid()
        {
            // Arrange
            var request = new ConsoleMeteringSubscribeRequest
            {
                Id = 1,
                Interval = 50,
                Binary = false,
                ChannelIndices = new[] { 0, 1, 2, 3 }
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().Contains("/console/metering/subscribe")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var act = async () => await _sut.PostMeteringSubscribeAsync(request);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task PostMeteringSubscribeAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var request = new ConsoleMeteringSubscribeRequest
            {
                Id = 1,
                Interval = 50,
                Binary = false,
                ChannelIndices = new[] { 0 }
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var act = async () => await _sut.PostMeteringSubscribeAsync(request);

            // Assert
            var exception = await act.Should().ThrowAsync<TransportException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task PostMeteringSubscribeAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var request = new ConsoleMeteringSubscribeRequest
            {
                Id = 1,
                Interval = 50,
                Binary = false,
                ChannelIndices = new[] { 0 }
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var act = async () => await _sut.PostMeteringSubscribeAsync(request);

            // Assert
            var exception = await act.Should().ThrowAsync<TransportException>();
            exception.Which.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /console/metering/unsubscribe
        // ========================================

        [Fact]
        public async Task PostMeteringUnsubscribeAsync_Success_ReturnsVoid()
        {
            // Arrange
            var request = new ConsoleMeteringUnsubscribeRequest
            {
                Id = 1
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().Contains("/console/metering/unsubscribe")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var act = async () => await _sut.PostMeteringUnsubscribeAsync(request);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task PostMeteringUnsubscribeAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var request = new ConsoleMeteringUnsubscribeRequest
            {
                Id = 1
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var act = async () => await _sut.PostMeteringUnsubscribeAsync(request);

            // Assert
            var exception = await act.Should().ThrowAsync<TransportException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostMeteringUnsubscribeAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var request = new ConsoleMeteringUnsubscribeRequest
            {
                Id = 1
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var act = async () => await _sut.PostMeteringUnsubscribeAsync(request);

            // Assert
            var exception = await act.Should().ThrowAsync<TransportException>();
            exception.Which.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /console/metering2/subscribe
        // ========================================

        [Fact]
        public async Task PostMetering2SubscribeAsync_Success_ReturnsVoid()
        {
            // Arrange
            var request = new ConsoleMeteringSubscribe2Request
            {
                Id = 2,
                Interval = 100,
                Binary = true,
                Params = new[]
                {
                    new MeteringParam { Index = 0, Type = 1 },
                    new MeteringParam { Index = 1, Type = 1 }
                }
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().Contains("/console/metering2/subscribe")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var act = async () => await _sut.PostMetering2SubscribeAsync(request);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task PostMetering2SubscribeAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var request = new ConsoleMeteringSubscribe2Request
            {
                Id = 2,
                Interval = 100,
                Binary = false,
                Params = new[] { new MeteringParam { Index = 0, Type = 1 } }
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var act = async () => await _sut.PostMetering2SubscribeAsync(request);

            // Assert
            var exception = await act.Should().ThrowAsync<TransportException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task PostMetering2SubscribeAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var request = new ConsoleMeteringSubscribe2Request
            {
                Id = 2,
                Interval = 100,
                Binary = false,
                Params = new[] { new MeteringParam { Index = 0, Type = 1 } }
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var act = async () => await _sut.PostMetering2SubscribeAsync(request);

            // Assert
            var exception = await act.Should().ThrowAsync<TransportException>();
            exception.Which.InnerException.Should().BeOfType<HttpRequestException>();
        }
    }
}
