/**
 * AppClient Phase 7 Tests - App Presets Endpoints
 * 
 * Phase 7 Coverage: App Presets Management (6 endpoints)
 * 1. GET /app/presets/scopes
 * 2. POST /app/presets/channel/apply
 * 3. POST /app/presets/channel/create
 * 4. POST /app/presets/scenes/apply
 * 5. POST /app/presets/scenes/create
 * 6. GET /app/presets/lastError
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
    public class AppClientPhase7Tests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly IAppClient _appClient;
        private readonly string _baseUrl = "http://localhost:8045";

        public AppClientPhase7Tests()
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
        // GET /app/presets/scopes Tests
        // ========================================

        [Fact]
        public async Task GetPresetsScopesAsync_Success_ReturnsScopes()
        {
            // Arrange
            var expectedResponse = new AppPresetsScopesResponse
            {
                Channel = new[]
                {
                    new AppPresetsScope { Name = "EQ", BitPos = 0 },
                    new AppPresetsScope { Name = "Gate", BitPos = 1 }
                },
                Global = new[]
                {
                    new AppPresetsScope { Name = "Main Mix", BitPos = 0 }
                }
            };

            var jsonResponse = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/presets/scopes"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _appClient.GetPresetsScopesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Channel.Should().HaveCount(2);
            result.Channel[0].Name.Should().Be("EQ");
            result.Channel[0].BitPos.Should().Be(0);
            result.Global.Should().HaveCount(1);
            result.Global[0].Name.Should().Be("Main Mix");
        }

        [Fact]
        public async Task GetPresetsScopesAsync_HttpError_ThrowsTransportException()
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
                async () => await _appClient.GetPresetsScopesAsync());

            exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetPresetsScopesAsync_NetworkError_ThrowsTransportException()
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
                async () => await _appClient.GetPresetsScopesAsync());

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /app/presets/channel/apply Tests
        // ========================================

        [Fact]
        public async Task PostPresetsChannelApplyAsync_Success_ReturnsVoid()
        {
            // Arrange
            var request = new AppPresetsChannelApplyRequest
            {
                Data = new { eq = new { gain = 0.5 } },
                Scope = 1,
                Channel = new ChannelReference { Offset = 0, Type = 1 }
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/presets/channel/apply"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            // Act
            Func<Task> act = async () => await _appClient.PostPresetsChannelApplyAsync(request);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task PostPresetsChannelApplyAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var request = new AppPresetsChannelApplyRequest
            {
                Data = new { },
                Scope = 1,
                Channel = new ChannelReference { Offset = 0, Type = 1 }
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
                async () => await _appClient.PostPresetsChannelApplyAsync(request));

            exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostPresetsChannelApplyAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var request = new AppPresetsChannelApplyRequest
            {
                Data = new { },
                Scope = 1,
                Channel = new ChannelReference { Offset = 0, Type = 1 }
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
                async () => await _appClient.PostPresetsChannelApplyAsync(request));

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /app/presets/channel/create Tests
        // ========================================

        [Fact]
        public async Task PostPresetsChannelCreateAsync_Success_ReturnsPresetData()
        {
            // Arrange
            var request = new AppPresetsChannelCreateRequest
            {
                Src = new ChannelReference { Offset = 0, Type = 1 },
                Scope = 1
            };

            var expectedResponse = new AppPresetsChannelCreateResponse
            {
                Data = new { eq = new { gain = 0.5 } },
                Scope = 1,
                Channel = new ChannelReference { Offset = 0, Type = 1 }
            };

            var jsonResponse = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/presets/channel/create"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _appClient.PostPresetsChannelCreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Scope.Should().Be(1);
            result.Channel.Offset.Should().Be(0);
            result.Channel.Type.Should().Be(1);
        }

        [Fact]
        public async Task PostPresetsChannelCreateAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var request = new AppPresetsChannelCreateRequest
            {
                Src = new ChannelReference { Offset = 0, Type = 1 },
                Scope = 1
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
                async () => await _appClient.PostPresetsChannelCreateAsync(request));

            exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostPresetsChannelCreateAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var request = new AppPresetsChannelCreateRequest
            {
                Src = new ChannelReference { Offset = 0, Type = 1 },
                Scope = 1
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
                async () => await _appClient.PostPresetsChannelCreateAsync(request));

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /app/presets/scenes/apply Tests
        // ========================================

        [Fact]
        public async Task PostPresetsScenesApplyAsync_Success_ReturnsVoid()
        {
            // Arrange
            var request = new AppPresetsSceneData
            {
                Data = new { scene = "Main Mix" },
                GlobalScope = 1,
                ChannelScopes = new[]
                {
                    new ChannelScopeMapping
                    {
                        Src = new ChannelReference { Offset = 0, Type = 1 },
                        Scope = 1,
                        Dest = new ChannelReference { Offset = 0, Type = 1 }
                    }
                }
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/presets/scenes/apply"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            // Act
            Func<Task> act = async () => await _appClient.PostPresetsScenesApplyAsync(request);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task PostPresetsScenesApplyAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var request = new AppPresetsSceneData
            {
                Data = new { },
                GlobalScope = 1,
                ChannelScopes = Array.Empty<ChannelScopeMapping>()
            };

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
                async () => await _appClient.PostPresetsScenesApplyAsync(request));

            exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task PostPresetsScenesApplyAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var request = new AppPresetsSceneData
            {
                Data = new { },
                GlobalScope = 1,
                ChannelScopes = Array.Empty<ChannelScopeMapping>()
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
                async () => await _appClient.PostPresetsScenesApplyAsync(request));

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /app/presets/scenes/create Tests
        // ========================================

        [Fact]
        public async Task PostPresetsScenesCreateAsync_Success_ReturnsSceneData()
        {
            // Arrange
            var expectedResponse = new AppPresetsSceneData
            {
                Data = new { scene = "Main Mix" },
                GlobalScope = 1,
                ChannelScopes = new[]
                {
                    new ChannelScopeMapping
                    {
                        Src = new ChannelReference { Offset = 0, Type = 1 },
                        Scope = 1,
                        Dest = new ChannelReference { Offset = 0, Type = 1 }
                    }
                }
            };

            var jsonResponse = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/presets/scenes/create"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _appClient.PostPresetsScenesCreateAsync();

            // Assert
            result.Should().NotBeNull();
            result.GlobalScope.Should().Be(1);
            result.ChannelScopes.Should().HaveCount(1);
        }

        [Fact]
        public async Task PostPresetsScenesCreateAsync_HttpError_ThrowsTransportException()
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
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _appClient.PostPresetsScenesCreateAsync());

            exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostPresetsScenesCreateAsync_NetworkError_ThrowsTransportException()
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
                async () => await _appClient.PostPresetsScenesCreateAsync());

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // GET /app/presets/lastError Tests
        // ========================================

        [Fact]
        public async Task GetPresetsLastErrorAsync_Success_ReturnsErrors()
        {
            // Arrange
            var expectedResponse = new AppPresetsLastErrorResponse
            {
                Warnings = new[] { "Channel 1 EQ out of range" },
                Errors = new[] { "Scene data corrupted" }
            };

            var jsonResponse = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/presets/lastError"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _appClient.GetPresetsLastErrorAsync();

            // Assert
            result.Should().NotBeNull();
            result.Warnings.Should().HaveCount(1);
            result.Warnings[0].Should().Be("Channel 1 EQ out of range");
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Should().Be("Scene data corrupted");
        }

        [Fact]
        public async Task GetPresetsLastErrorAsync_HttpError_ThrowsTransportException()
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
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                async () => await _appClient.GetPresetsLastErrorAsync());

            exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPresetsLastErrorAsync_NetworkError_ThrowsTransportException()
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
                async () => await _appClient.GetPresetsLastErrorAsync());

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }
    }
}
