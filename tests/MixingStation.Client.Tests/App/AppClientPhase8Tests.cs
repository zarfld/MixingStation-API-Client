/**
 * AppClient Phase 8 Tests - App IDCA & UI Endpoints
 * 
 * Phase 8 Coverage: App IDCA & UI Management (6 endpoints)
 * 1. POST /app/idcas (create IDCA)
 * 2. POST /app/idcas/{index} (modify IDCA)
 * 3. POST /app/idcas/{index}/delete
 * 4. POST /app/idcas/rearrange
 * 5. GET /app/ui/selectedChannel
 * 6. GET /app/ui/selectedChannel/{nameOrIndex}
 * 
 * Test Strategy: TDD Red-Green-Refactor
 * - Success: Mock HttpMessageHandler, return appropriate status with JSON
 * - HTTP Error: Return error status, assert StatusCode == HttpStatusCode.X
 * - Network Error: Throw HttpRequestException, assert TransportException.InnerException type
 * 
 * Pattern: Each endpoint has 3 tests (Success, HTTP Error, Network Error)
 * Expected: 18 tests total (6 endpoints × 3 tests)
 */

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using MixingStation.Client.App;
using MixingStation.Client.Exceptions;
using MixingStation.Client.Models;
using Xunit;

namespace MixingStation.Client.Tests.App
{
    public class AppClientPhase8Tests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly IAppClient _appClient;
        private readonly string _baseUrl = "http://localhost:8045";

        public AppClientPhase8Tests()
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
            _appClient = new AppClient(serviceProvider.GetRequiredService<IHttpClientFactory>());
        }

        // ========================================
        // POST /app/idcas (Create IDCA) Tests
        // ========================================

        [Fact]
        public async Task PostIdcasAsync_Create_Success_ReturnsIdca()
        {
            // Arrange
            var request = new AppIdcaRequest
            {
                Members = new[]
                {
                    new ChannelReference { Offset = 0, Type = 1 },
                    new ChannelReference { Offset = 1, Type = 1 }
                }
            };

            var expectedResponse = new AppIdcaResponse
            {
                Members = request.Members,
                Index = 0
            };

            var jsonResponse = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/idcas"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _appClient.PostIdcasAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Index.Should().Be(0);
            result.Members.Should().HaveCount(2);
        }

        [Fact]
        public async Task PostIdcasAsync_Create_HttpError_ThrowsTransportException()
        {
            // Arrange
            var request = new AppIdcaRequest
            {
                Members = Array.Empty<ChannelReference>()
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _appClient.PostIdcasAsync(request));

            exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostIdcasAsync_Create_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var request = new AppIdcaRequest
            {
                Members = Array.Empty<ChannelReference>()
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network failure"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _appClient.PostIdcasAsync(request));

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /app/idcas/{index} (Modify IDCA) Tests
        // ========================================

        [Fact]
        public async Task PostIdcasAsync_Modify_Success_ReturnsIdca()
        {
            // Arrange
            var index = "0";
            var request = new AppIdcaRequest
            {
                Members = new[]
                {
                    new ChannelReference { Offset = 2, Type = 1 }
                }
            };

            var expectedResponse = new AppIdcaResponse
            {
                Members = request.Members,
                Index = 0
            };

            var jsonResponse = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/idcas/{index}"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _appClient.PostIdcasAsync(index, request);

            // Assert
            result.Should().NotBeNull();
            result.Index.Should().Be(0);
            result.Members.Should().HaveCount(1);
        }

        [Fact]
        public async Task PostIdcasAsync_Modify_HttpError_ThrowsTransportException()
        {
            // Arrange
            var index = "999";
            var request = new AppIdcaRequest
            {
                Members = Array.Empty<ChannelReference>()
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _appClient.PostIdcasAsync(index, request));

            exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostIdcasAsync_Modify_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var index = "0";
            var request = new AppIdcaRequest
            {
                Members = Array.Empty<ChannelReference>()
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network failure"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _appClient.PostIdcasAsync(index, request));

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /app/idcas/{index}/delete Tests
        // ========================================

        [Fact]
        public async Task PostIdcasDeleteAsync_Success_ReturnsVoid()
        {
            // Arrange
            var index = "0";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/idcas/{index}/delete"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            // Act
            Func<Task> act = async () => await _appClient.PostIdcasDeleteAsync(index);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task PostIdcasDeleteAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var index = "999";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _appClient.PostIdcasDeleteAsync(index));

            exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostIdcasDeleteAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var index = "0";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network failure"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _appClient.PostIdcasDeleteAsync(index));

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /app/idcas/rearrange Tests
        // ========================================

        [Fact]
        public async Task PostIdcasRearrangeAsync_Success_ReturnsIdcas()
        {
            // Arrange
            var request = new AppIdcaRearrangeRequest
            {
                NewIndices = new[] { 1, 0, 2 }
            };

            var expectedResponse = new AppIdcaRearrangeResponse
            {
                Dcas = new[]
                {
                    new AppIdcaResponse { Members = Array.Empty<ChannelReference>(), Index = 0 },
                    new AppIdcaResponse { Members = Array.Empty<ChannelReference>(), Index = 1 },
                    new AppIdcaResponse { Members = Array.Empty<ChannelReference>(), Index = 2 }
                }
            };

            var jsonResponse = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/idcas/rearrange"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _appClient.PostIdcasRearrangeAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Dcas.Should().HaveCount(3);
        }

        [Fact]
        public async Task PostIdcasRearrangeAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var request = new AppIdcaRearrangeRequest
            {
                NewIndices = Array.Empty<int>()
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _appClient.PostIdcasRearrangeAsync(request));

            exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostIdcasRearrangeAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var request = new AppIdcaRearrangeRequest
            {
                NewIndices = Array.Empty<int>()
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network failure"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _appClient.PostIdcasRearrangeAsync(request));

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // GET /app/ui/selectedChannel Tests
        // ========================================

        [Fact]
        public async Task GetUiSelectedChannelAsync_NoParam_Success_ReturnsChannel()
        {
            // Arrange
            var expectedResponse = new AppUiSelectedChannelResponse
            {
                GenericName = "Input 1",
                Name = "Vocals",
                Index = 0
            };

            var jsonResponse = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/ui/selectedChannel"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _appClient.GetUiSelectedChannelAsync();

            // Assert
            result.Should().NotBeNull();
            result.GenericName.Should().Be("Input 1");
            result.Name.Should().Be("Vocals");
            result.Index.Should().Be(0);
        }

        [Fact]
        public async Task GetUiSelectedChannelAsync_NoParam_HttpError_ThrowsTransportException()
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
                async () => await _appClient.GetUiSelectedChannelAsync());

            exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetUiSelectedChannelAsync_NoParam_NetworkError_ThrowsTransportException()
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
                async () => await _appClient.GetUiSelectedChannelAsync());

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // GET /app/ui/selectedChannel/{nameOrIndex} Tests
        // ========================================

        [Fact]
        public async Task GetUiSelectedChannelAsync_WithParam_Success_ReturnsChannel()
        {
            // Arrange
            var nameOrIndex = "Drums";
            var expectedResponse = new AppUiSelectedChannelResponse
            {
                GenericName = "Input 2",
                Name = "Drums",
                Index = 1
            };

            var jsonResponse = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/ui/selectedChannel/{nameOrIndex}"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _appClient.GetUiSelectedChannelAsync(nameOrIndex);

            // Assert
            result.Should().NotBeNull();
            result.GenericName.Should().Be("Input 2");
            result.Name.Should().Be("Drums");
            result.Index.Should().Be(1);
        }

        [Fact]
        public async Task GetUiSelectedChannelAsync_WithParam_HttpError_ThrowsTransportException()
        {
            // Arrange
            var nameOrIndex = "NonExistent";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _appClient.GetUiSelectedChannelAsync(nameOrIndex));

            exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUiSelectedChannelAsync_WithParam_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var nameOrIndex = "Vocals";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network failure"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _appClient.GetUiSelectedChannelAsync(nameOrIndex));

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }
    }
}
