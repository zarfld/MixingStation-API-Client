/**
 * ConsoleClientTests - TDD test suite for ConsoleClient
 * 
 * Verifies:
 * - #4 REQ-F-001: HTTP Client Transport
 * - #6 REQ-F-003: Console Data Reading
 * 
 * Test Type: Unit + Integration
 * Coverage Target: >80%
 * Framework: xUnit + Moq + FluentAssertions
 * 
 * TDD Cycle: RED → GREEN → REFACTOR
 * 
 * OpenAPI Reference: docs/openapi.json
 * Endpoints:
 * - GET /console/information (blob-bx-b)
 * - GET /console/data/get/{path}/{format} (blob-bx-m)
 * 
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/4
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/6
 */

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Http;
using Moq;
using Moq.Protected;
using MixingStation.Client.Console;
using MixingStation.Client.Exceptions;
using MixingStation.Client.Models;
using Xunit;

namespace MixingStation.Client.Tests.Console
{
    /// <summary>
    /// TDD test suite for ConsoleClient.
    /// Tests written BEFORE implementation (RED phase).
    /// All tests use REAL schemas from docs/openapi.json (ADR-002 compliance).
    /// </summary>
    public class ConsoleClientTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public ConsoleClientTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        }

        // ========================================
        // Constructor Tests
        // ========================================

        [Fact]
        public void Constructor_WithNullHttpClientFactory_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new ConsoleClient(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("httpClientFactory");
        }

        // ========================================
        // GetInformationAsync Tests (GET /console/information)
        // ========================================

        [Fact]
        public async Task GetInformationAsync_WithValidResponse_ReturnsConsoleInformation()
        {
            // Arrange
            var expectedResponse = new ConsoleInformationResponse
            {
                TotalChannels = 32,
                ChannelColors = new[]
                {
                    new ChannelColor { Id = 0, Name = "Red", Hex = "#FF0000" },
                    new ChannelColor { Id = 1, Name = "Blue", Hex = "#0000FF" }
                },
                ChannelTypes = new[]
                {
                    new ChannelType { Id = 0, Name = "Input", Count = 16 },
                    new ChannelType { Id = 1, Name = "Aux", Count = 8 }
                },
                RtaFrequencies = new[] { 31.5f, 63f, 125f, 250f, 500f, 1000f, 2000f, 4000f, 8000f, 16000f },
                DbfsOffset = -18.0
            };

            var jsonResponse = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().EndsWith("/console/information")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            var result = await client.GetInformationAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalChannels.Should().Be(32);
            result.ChannelColors.Should().HaveCount(2);
            result.ChannelColors[0].Name.Should().Be("Red");
            result.ChannelTypes.Should().HaveCount(2);
            result.ChannelTypes[0].Name.Should().Be("Input");
            result.RtaFrequencies.Should().HaveCount(10);
            result.DbfsOffset.Should().Be(-18.0);
        }

        [Fact]
        public async Task GetInformationAsync_WithUnauthorized_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent("Unauthorized")
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.GetInformationAsync();

            // Assert
            await act.Should().ThrowAsync<TransportException>()
                .Where(ex => ex.StatusCode == HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetInformationAsync_WithNetworkError_ThrowsTransportException()
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

            // Act
            Func<Task> act = async () => await client.GetInformationAsync();

            // Assert
            await act.Should().ThrowAsync<TransportException>()
                .WithMessage("*Network error*");
        }

        [Fact]
        public async Task GetInformationAsync_WithMalformedJson_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{ invalid json }")
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.GetInformationAsync();

            // Assert
            await act.Should().ThrowAsync<TransportException>()
                .WithMessage("*Failed to deserialize*");
        }

        // ========================================
        // GetDataGetAsync Tests (GET /console/data/get/{path}/{format})
        // ========================================

        [Theory]
        [InlineData("ch.0.name", "val", "Channel 1")]
        [InlineData("ch.0.config.gain", "val", -12.5)]
        [InlineData("ch.0.config.gain", "norm", 0.5)]
        [InlineData("ch.1.mute", "val", true)]
        public async Task GetDataGetAsync_WithValidResponse_ReturnsConsoleDataValue(
            string path, 
            string format, 
            object expectedValue)
        {
            // Arrange
            var expectedResponse = new ConsoleDataValue
            {
                Format = format,
                Value = expectedValue
            };

            var jsonResponse = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().EndsWith($"/console/data/get/{path}/{format}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            var result = await client.GetDataGetAsync(path, format);

            // Assert
            result.Should().NotBeNull();
            result.Format.Should().Be(format);
            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDataGetAsync_WithNullPath_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.GetDataGetAsync(null!, "val");

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("path");
        }

        [Fact]
        public async Task GetDataGetAsync_WithNullFormat_ThrowsArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.GetDataGetAsync("ch.0.name", null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("format");
        }

        [Fact]
        public async Task GetDataGetAsync_WithBadRequest_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Invalid path")
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.GetDataGetAsync("invalid.path", "val");

            // Assert
            await act.Should().ThrowAsync<TransportException>()
                .Where(ex => ex.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetDataGetAsync_WithNotFound_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("Path not found")
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.GetDataGetAsync("ch.999.name", "val");

            // Assert
            await act.Should().ThrowAsync<TransportException>()
                .Where(ex => ex.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetDataGetAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new OperationCanceledException());

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8045/")
            };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.GetDataGetAsync("ch.0.name", "val", cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
