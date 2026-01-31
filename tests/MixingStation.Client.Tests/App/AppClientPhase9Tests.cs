/**
 * AppClient Phase 9 Tests - App Network & Misc Endpoints
 * 
 * Phase 9 Coverage: App Network & Misc (3 endpoints)
 * 1. GET /app/network/interfaces
 * 2. POST /app/network/interfaces/primary
 * 3. POST /app/save
 * 
 * Test Strategy: TDD Red-Green-Refactor
 * - Success: Mock HttpMessageHandler, return appropriate status with JSON
 * - HTTP Error: Return error status, assert StatusCode == HttpStatusCode.X
 * - Network Error: Throw HttpRequestException, assert TransportException.InnerException type
 * 
 * Pattern: Each endpoint has 3 tests (Success, HTTP Error, Network Error)
 * Expected: 9 tests total (3 endpoints × 3 tests)
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
    public class AppClientPhase9Tests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly IAppClient _appClient;
        private readonly string _baseUrl = "http://localhost:8045";

        public AppClientPhase9Tests()
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
        // GET /app/network/interfaces Tests
        // ========================================

        [Fact]
        public async Task GetNetworkInterfacesAsync_Success_ReturnsInterfaces()
        {
            // Arrange
            var expectedResponse = new NetworkInterfacesResponse
            {
                Interfaces = new[]
                {
                    new NetworkInterface
                    {
                        DisplayName = "Ethernet",
                        IsPrimary = true,
                        Name = "eth0",
                        IpAddress = "192.168.1.100",
                        SubnetMask = "255.255.255.0",
                        OverridePrimary = false
                    },
                    new NetworkInterface
                    {
                        DisplayName = "Wi-Fi",
                        IsPrimary = false,
                        Name = "wlan0",
                        IpAddress = "192.168.1.101",
                        SubnetMask = "255.255.255.0",
                        OverridePrimary = false
                    }
                }
            };

            var jsonResponse = JsonSerializer.Serialize(expectedResponse, AppClient.JsonOptions);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/network/interfaces"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _appClient.GetNetworkInterfacesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Interfaces.Should().HaveCount(2);
            result.Interfaces[0].DisplayName.Should().Be("Ethernet");
            result.Interfaces[0].IsPrimary.Should().BeTrue();
            result.Interfaces[1].DisplayName.Should().Be("Wi-Fi");
            result.Interfaces[1].IsPrimary.Should().BeFalse();
        }

        [Fact]
        public async Task GetNetworkInterfacesAsync_HttpError_ThrowsTransportException()
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
                async () => await _appClient.GetNetworkInterfacesAsync());

            exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetNetworkInterfacesAsync_NetworkError_ThrowsTransportException()
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
                async () => await _appClient.GetNetworkInterfacesAsync());

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /app/network/interfaces/primary Tests
        // ========================================

        [Fact]
        public async Task PostNetworkInterfacesPrimaryAsync_Success_ReturnsInterfaces()
        {
            // Arrange
            var request = new NetworkInterfacePrimaryRequest
            {
                Name = "wlan0"
            };

            var expectedResponse = new NetworkInterfacesResponse
            {
                Interfaces = new[]
                {
                    new NetworkInterface
                    {
                        DisplayName = "Ethernet",
                        IsPrimary = false,
                        Name = "eth0",
                        IpAddress = "192.168.1.100",
                        SubnetMask = "255.255.255.0",
                        OverridePrimary = false
                    },
                    new NetworkInterface
                    {
                        DisplayName = "Wi-Fi",
                        IsPrimary = true,
                        Name = "wlan0",
                        IpAddress = "192.168.1.101",
                        SubnetMask = "255.255.255.0",
                        OverridePrimary = true
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
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/network/interfaces/primary"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _appClient.PostNetworkInterfacesPrimaryAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Interfaces.Should().HaveCount(2);
            result.Interfaces[1].Name.Should().Be("wlan0");
            result.Interfaces[1].IsPrimary.Should().BeTrue();
            result.Interfaces[1].OverridePrimary.Should().BeTrue();
        }

        [Fact]
        public async Task PostNetworkInterfacesPrimaryAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            var request = new NetworkInterfacePrimaryRequest
            {
                Name = "nonexistent"
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
                async () => await _appClient.PostNetworkInterfacesPrimaryAsync(request));

            exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostNetworkInterfacesPrimaryAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            var request = new NetworkInterfacePrimaryRequest
            {
                Name = "eth0"
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
                async () => await _appClient.PostNetworkInterfacesPrimaryAsync(request));

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /app/save Tests
        // ========================================

        [Fact]
        public async Task PostSaveAsync_Success_ReturnsVoid()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.ToString() == $"{_baseUrl}/app/save"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            // Act
            Func<Task> act = async () => await _appClient.PostSaveAsync();

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task PostSaveAsync_HttpError_ThrowsTransportException()
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
                async () => await _appClient.PostSaveAsync());

            exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task PostSaveAsync_NetworkError_ThrowsTransportException()
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
                async () => await _appClient.PostSaveAsync());

            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }
    }
}
