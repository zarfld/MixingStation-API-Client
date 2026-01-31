/**
 * ConsoleClientPhase2Tests - TDD test suite for ConsoleClient Phase 2 Endpoints
 * 
 * Verifies:
 * - #TBD REQ-F-XXX: Console Data Subscription (WebSocket)
 * - #TBD REQ-F-XXX: Console Data Writing
 * 
 * Test Type: Unit + Integration
 * Coverage Target: >80%
 * Framework: xUnit + Moq + FluentAssertions
 * 
 * TDD Cycle: RED → GREEN → REFACTOR
 * 
 * OpenAPI Reference: docs/openapi.json
 * Endpoints:
 * - POST /console/data/subscribe (blob-bw-j → 204 No Content)
 * - POST /console/data/set/{path}/{format} (blob-bx-m → blob-bx-m)
 */

using System;
using System.Net;
using System.Net.Http;
using System.Text;
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
    /// <summary>
    /// TDD test suite for ConsoleClient Phase 2 endpoints.
    /// Tests written BEFORE implementation (RED phase).
    /// All tests use REAL schemas from docs/openapi.json (ADR-002 compliance).
    /// </summary>
    public class ConsoleClientPhase2Tests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public ConsoleClientPhase2Tests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        }

        // ========================================
        // POST /console/data/subscribe Tests (Phase 2)
        // OpenAPI: blob-bw-j request → 204 No Content
        // ========================================

        [Fact]
        public async Task PostDataSubscribeAsync_WithValidRequest_Returns204NoContent()
        {
            // Arrange
            var request = new ConsoleDataSubscribeRequest
            {
                Path = "ch.0.name",
                Format = "val"
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Post &&
                        r.RequestUri!.PathAndQuery == "/console/data/subscribe"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            await client.PostDataSubscribeAsync(request);

            // Assert
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Post &&
                    r.RequestUri!.PathAndQuery == "/console/data/subscribe"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task PostDataSubscribeAsync_WithWildcardPattern_Returns204NoContent()
        {
            // Arrange
            var request = new ConsoleDataSubscribeRequest
            {
                Path = "ch.*.name",
                Format = "val"
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            await client.PostDataSubscribeAsync(request);

            // Assert
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task PostDataSubscribeAsync_WithHttpError_ThrowsTransportException()
        {
            // Arrange
            var request = new ConsoleDataSubscribeRequest { Path = "invalid", Format = "val" };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act & Assert
            Func<Task> act = async () => await client.PostDataSubscribeAsync(request);
            await act.Should().ThrowAsync<TransportException>()
                .Where(ex => ex.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostDataSubscribeAsync_WithNetworkError_ThrowsTransportException()
        {
            // Arrange
            var request = new ConsoleDataSubscribeRequest { Path = "ch.0.name", Format = "val" };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                () => client.PostDataSubscribeAsync(request));
            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        [Fact]
        public async Task PostDataSubscribeAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.PostDataSubscribeAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("request");
        }

        // ========================================
        // POST /console/data/set/{path}/{format} Tests (Phase 2)
        // OpenAPI: blob-bx-m request → blob-bx-m response (200 OK)
        // ========================================

        [Fact]
        public async Task PostDataSetAsync_WithValidRequest_ReturnsUpdatedValue()
        {
            // Arrange
            var responseValue = new ConsoleDataValue
            {
                Format = "val",
                Value = 0.75
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Post &&
                        r.RequestUri!.PathAndQuery == "/console/data/set/ch.0.config.gain/val"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(responseValue, MixingStation.Client.App.AppClient.JsonOptions),
                        Encoding.UTF8,
                        System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"))
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            var requestValue = new ConsoleDataValue
            {
                Format = "val",
                Value = 0.75
            };

            // Act
            var result = await client.PostDataSetAsync("ch.0.config.gain", "val", requestValue);

            // Assert
            result.Should().NotBeNull();
            result.Format.Should().Be("val");
            result.Value.Should().BeOfType<JsonElement>();
            ((JsonElement)result.Value!).GetDouble().Should().Be(0.75);
        }

        [Fact]
        public async Task PostDataSetAsync_WithNormalizedFormat_WorksCorrectly()
        {
            // Arrange
            var responseValue = new ConsoleDataValue
            {
                Format = "norm",
                Value = 0.5
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(responseValue, MixingStation.Client.App.AppClient.JsonOptions),
                        Encoding.UTF8,
                        System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"))
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            var requestValue = new ConsoleDataValue
            {
                Format = "norm",
                Value = 0.5
            };

            // Act
            var result = await client.PostDataSetAsync("ch.0.mute", "norm", requestValue);

            // Assert
            result.Format.Should().Be("norm");
            result.Value.Should().BeOfType<JsonElement>();
            ((JsonElement)result.Value!).GetDouble().Should().Be(0.5);
        }

        [Fact]
        public async Task PostDataSetAsync_WithBadRequest_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);
            var requestValue = new ConsoleDataValue { Format = "val", Value = "invalid" };

            // Act & Assert
            Func<Task> act = async () => await client.PostDataSetAsync("ch.0.config.gain", "val", requestValue);
            await act.Should().ThrowAsync<TransportException>()
                .Where(ex => ex.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostDataSetAsync_WithNotFound_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);
            var requestValue = new ConsoleDataValue { Format = "val", Value = 0.5 };

            // Act & Assert
            Func<Task> act = async () => await client.PostDataSetAsync("nonexistent.path", "val", requestValue);
            await act.Should().ThrowAsync<TransportException>()
                .Where(ex => ex.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostDataSetAsync_WithServerError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);
            var requestValue = new ConsoleDataValue { Format = "val", Value = 0.5 };

            // Act & Assert
            Func<Task> act = async () => await client.PostDataSetAsync("ch.0.config.gain", "val", requestValue);
            await act.Should().ThrowAsync<TransportException>()
                .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task PostDataSetAsync_WithNetworkError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);
            var requestValue = new ConsoleDataValue { Format = "val", Value = 0.5 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                () => client.PostDataSetAsync("ch.0.config.gain", "val", requestValue));
            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        [Fact]
        public async Task PostDataSetAsync_WithNullPath_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);
            var requestValue = new ConsoleDataValue { Format = "val", Value = 0.5 };

            // Act
            Func<Task> act = async () => await client.PostDataSetAsync(null!, "val", requestValue);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("path");
        }

        [Fact]
        public async Task PostDataSetAsync_WithNullFormat_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);
            var requestValue = new ConsoleDataValue { Format = "val", Value = 0.5 };

            // Act
            Func<Task> act = async () => await client.PostDataSetAsync("ch.0.config.gain", null!, requestValue);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("format");
        }

        [Fact]
        public async Task PostDataSetAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.PostDataSetAsync("ch.0.config.gain", "val", null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("request");
        }
    }
}
