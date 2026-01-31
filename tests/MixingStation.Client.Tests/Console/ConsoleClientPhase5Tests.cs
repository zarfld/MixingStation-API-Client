/**
 * ConsoleClientPhase5Tests - TDD test suite for Phase 5 endpoints
 * 
 * Phase 5: Console Authentication & Mix Targets (3 endpoints)
 * - GET /console/auth/info
 * - POST /console/auth/login
 * - GET /console/mixTargets
 * 
 * Test Pattern: 3 tests per endpoint (success, HTTP error, network error)
 * Expected: 9 tests total
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
using Microsoft.Extensions.Http;
using Moq;
using Moq.Protected;
using MixingStation.Client.App;
using MixingStation.Client.Console;
using MixingStation.Client.Exceptions;
using MixingStation.Client.Models;
using Xunit;

namespace MixingStation.Client.Tests.Console
{
    public class ConsoleClientPhase5Tests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly ConsoleClient _sut;

        public ConsoleClientPhase5Tests()
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
        // GET /console/auth/info
        // ========================================

        [Fact]
        public async Task GetAuthInfoAsync_ReturnsAuthInfo()
        {
            // Arrange
            var expectedResponse = new ConsoleAuthInfoResponse
            {
                Users = new[] { "admin", "user1", "user2" }
            };

            var responseContent = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"))
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains("/console/auth/info")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.GetAuthInfoAsync();

            // Assert
            result.Should().NotBeNull();
            result.Users.Should().BeEquivalentTo(expectedResponse.Users);
        }

        [Fact]
        public async Task GetAuthInfoAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var act = async () => await _sut.GetAuthInfoAsync();

            // Assert
            var exception = await act.Should().ThrowAsync<TransportException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetAuthInfoAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var act = async () => await _sut.GetAuthInfoAsync();

            // Assert
            var exception = await act.Should().ThrowAsync<TransportException>();
            exception.Which.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /console/auth/login
        // ========================================

        [Fact]
        public async Task PostAuthLoginAsync_ReturnsLoginResult()
        {
            // Arrange
            var request = new ConsoleAuthLoginRequest
            {
                User = "admin",
                Password = "secret123"
            };

            var expectedResponse = new ConsoleAuthLoginResponse
            {
                Success = true
            };

            var responseContent = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"))
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString().Contains("/console/auth/login")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.PostAuthLoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task PostAuthLoginAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var request = new ConsoleAuthLoginRequest
            {
                User = "admin",
                Password = "wrong"
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var act = async () => await _sut.PostAuthLoginAsync(request);

            // Assert
            var exception = await act.Should().ThrowAsync<TransportException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PostAuthLoginAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var request = new ConsoleAuthLoginRequest
            {
                User = "admin",
                Password = "secret123"
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var act = async () => await _sut.PostAuthLoginAsync(request);

            // Assert
            var exception = await act.Should().ThrowAsync<TransportException>();
            exception.Which.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // GET /console/mixTargets
        // ========================================

        [Fact]
        public async Task GetMixTargetsAsync_ReturnsMixTargets()
        {
            // Arrange
            var expectedResponse = new ConsoleMixTargetsResponse
            {
                Targets = new[]
                {
                    new MixTarget
                    {
                        IsChannel = true,
                        Name = "Main L/R",
                        ChannelType = new MixTargetChannelType
                        {
                            Offset = 0,
                            Stereo = true,
                            Name = "Main",
                            Count = 1,
                            ShortName = "M",
                            Type = 1
                        },
                        Id = 100,
                        ChannelIndex = 0
                    },
                    new MixTarget
                    {
                        IsChannel = false,
                        Name = "AUX 1",
                        Id = 101,
                        ChannelIndex = 1
                    }
                }
            };

            var responseContent = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"))
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains("/console/mixTargets")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.GetMixTargetsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Targets.Should().HaveCount(2);
            result.Targets[0].Name.Should().Be("Main L/R");
            result.Targets[0].IsChannel.Should().BeTrue();
            result.Targets[0].ChannelType.Should().NotBeNull();
            result.Targets[0].ChannelType!.Stereo.Should().BeTrue();
        }

        [Fact]
        public async Task GetMixTargetsAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var act = async () => await _sut.GetMixTargetsAsync();

            // Assert
            var exception = await act.Should().ThrowAsync<TransportException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetMixTargetsAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var act = async () => await _sut.GetMixTargetsAsync();

            // Assert
            var exception = await act.Should().ThrowAsync<TransportException>();
            exception.Which.InnerException.Should().BeOfType<HttpRequestException>();
        }
    }
}
