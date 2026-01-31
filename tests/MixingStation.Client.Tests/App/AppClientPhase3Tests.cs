/**
 * AppClientPhase3Tests - TDD test suite for Phase 3 mixer lifecycle endpoints
 * 
 * Test Coverage: Phase 3 Endpoints (Mixer Lifecycle)
 * - GET /app/mixers/available (blob-ca-d)
 * - POST /app/mixers/search (blob-ca-f → 204)
 * - GET /app/mixers/searchResults (blob-ca-e)
 * - POST /app/mixers/disconnect (204)
 * - POST /app/mixers/offline (blob-ca-g → 204)
 * 
 * TDD Methodology: RED → GREEN → REFACTOR
 * Phase: RED (tests written first, implementation follows)
 * 
 * Architecture:
 * - #11 ADR-002: HTTP Transport + REST Mirror Naming Policy
 * 
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/11
 */

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MixingStation.Client.App;
using MixingStation.Client.Exceptions;
using MixingStation.Client.Models;
using Moq;
using Moq.Protected;
using Xunit;

namespace MixingStation.Client.Tests.App
{
    public class AppClientPhase3Tests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly AppClient _client;
        private const string BaseUrl = "http://localhost:8045";

        public AppClientPhase3Tests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri(BaseUrl)
            };

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            _client = new AppClient(_httpClientFactoryMock.Object);
        }

        // ========================================
        // GET /app/mixers/available Tests
        // ========================================

        [Fact]
        public async Task GetMixersAvailableAsync_ReturnsAvailableMixers()
        {
            // Arrange
            var expectedResponse = new AppMixersAvailableResponse
            {
                Consoles = new[]
                {
                    new MixerConsole
                    {
                        ConsoleId = 1,
                        Name = "Behringer X32",
                        Models = new[] { "X32", "X32 Compact" },
                        SupportedHardwareModels = new[] { "X32", "X32 Compact", "X32 Producer" },
                        ManufacturerId = 1
                    },
                    new MixerConsole
                    {
                        ConsoleId = 2,
                        Name = "Midas M32",
                        Models = new[] { "M32", "M32R" },
                        SupportedHardwareModels = new[] { "M32", "M32R" },
                        ManufacturerId = 2
                    }
                }
            };

            var responseJson = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8,
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == $"{BaseUrl}/app/mixers/available"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _client.GetMixersAvailableAsync();

            // Assert
            result.Should().NotBeNull();
            result.Consoles.Should().HaveCount(2);
            result.Consoles[0].ConsoleId.Should().Be(1);
            result.Consoles[0].Name.Should().Be("Behringer X32");
            result.Consoles[0].Models.Should().Contain("X32");
            result.Consoles[1].ConsoleId.Should().Be(2);
            result.Consoles[1].Name.Should().Be("Midas M32");
        }

        [Fact]
        public async Task GetMixersAvailableAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{\"error\":\"Server error\"}")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _client.GetMixersAvailableAsync());

            exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetMixersAvailableAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network unreachable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _client.GetMixersAvailableAsync());

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /app/mixers/search Tests
        // ========================================

        [Fact]
        public async Task PostMixersSearchAsync_ValidRequest_Succeeds()
        {
            // Arrange
            var request = new AppMixersSearchRequest { ConsoleId = 1 };
            var mockResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == $"{BaseUrl}/app/mixers/search" &&
                        req.Content != null),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act
            await _client.PostMixersSearchAsync(request);

            // Assert - no exception thrown
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task PostMixersSearchAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var request = new AppMixersSearchRequest { ConsoleId = 1 };
            var mockResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _client.PostMixersSearchAsync(request));

            exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostMixersSearchAsync_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _client.PostMixersSearchAsync(null!));
        }

        // ========================================
        // GET /app/mixers/searchResults Tests
        // ========================================

        [Fact]
        public async Task GetMixersSearchResultsAsync_ReturnsSearchResults()
        {
            // Arrange
            var expectedResponse = new AppMixersSearchResultsResponse
            {
                Results = new[]
                {
                    new MixerSearchResult
                    {
                        ModelId = 1,
                        Ip = "192.168.1.100",
                        Name = "X32-Studio",
                        Model = "X32",
                        Version = "4.06"
                    },
                    new MixerSearchResult
                    {
                        ModelId = 2,
                        Ip = "192.168.1.101",
                        Name = "M32-Live",
                        Model = "M32",
                        Version = "4.01"
                    }
                }
            };

            var responseJson = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8,
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == $"{BaseUrl}/app/mixers/searchResults"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _client.GetMixersSearchResultsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Results.Should().HaveCount(2);
            result.Results[0].Ip.Should().Be("192.168.1.100");
            result.Results[0].Name.Should().Be("X32-Studio");
            result.Results[1].Ip.Should().Be("192.168.1.101");
        }

        [Fact]
        public async Task GetMixersSearchResultsAsync_EmptyResults_ReturnsEmptyArray()
        {
            // Arrange
            var expectedResponse = new AppMixersSearchResultsResponse
            {
                Results = Array.Empty<MixerSearchResult>()
            };

            var responseJson = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8,
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"))
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _client.GetMixersSearchResultsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMixersSearchResultsAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _client.GetMixersSearchResultsAsync());

            exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ========================================
        // POST /app/mixers/disconnect Tests
        // ========================================

        [Fact]
        public async Task PostMixersDisconnectAsync_Succeeds()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == $"{BaseUrl}/app/mixers/disconnect"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act
            await _client.PostMixersDisconnectAsync();

            // Assert - no exception thrown
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task PostMixersDisconnectAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _client.PostMixersDisconnectAsync());

            exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        // ========================================
        // POST /app/mixers/offline Tests
        // ========================================

        [Fact]
        public async Task PostMixersOfflineAsync_ValidRequest_Succeeds()
        {
            // Arrange
            var request = new AppMixersOfflineRequest
            {
                ConsoleId = 1,
                ModelId = 5,
                Model = "X32"
            };
            var mockResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == $"{BaseUrl}/app/mixers/offline" &&
                        req.Content != null),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act
            await _client.PostMixersOfflineAsync(request);

            // Assert - no exception thrown
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task PostMixersOfflineAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var request = new AppMixersOfflineRequest
            {
                ConsoleId = 1,
                ModelId = 5,
                Model = "X32"
            };
            var mockResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _client.PostMixersOfflineAsync(request));

            exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostMixersOfflineAsync_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _client.PostMixersOfflineAsync(null!));
        }

        [Fact]
        public async Task PostMixersOfflineAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var request = new AppMixersOfflineRequest
            {
                ConsoleId = 1,
                ModelId = 5,
                Model = "X32"
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection refused"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _client.PostMixersOfflineAsync(request));

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }
    }
}
