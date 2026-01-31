/**
 * ConsoleClientPhase4Tests - TDD test suite for Phase 4 Console Data Discovery endpoints
 * 
 * SOURCE OF TRUTH: docs/openapi.json (http://localhost:8045/openapi.json)
 * Test Phase: RED → GREEN
 * 
 * Phase 4 Endpoints (6 total):
 * - GET /console/data/categories → GetDataCategoriesAsync()
 * - GET /console/data/paths → GetDataPathsAsync()
 * - GET /console/data/paths/{path} → GetDataPathsAsync(path)
 * - GET /console/data/definitions/{path} → GetDataDefinitionsAsync(path) [DEPRECATED]
 * - GET /console/data/definitions2/{path} → GetDataDefinitions2Async(path)
 * - POST /console/data/unsubscribe → PostDataUnsubscribeAsync(request)
 * 
 * Verifies:
 * - #6 REQ-F-003: Console Data Reading
 * - #11 ADR-002: HTTP Transport + REST Mirror Naming Policy
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MixingStation.Client.App;
using MixingStation.Client.Console;
using MixingStation.Client.Exceptions;
using MixingStation.Client.Models;
using Moq;
using Moq.Protected;
using Xunit;

namespace MixingStation.Client.Tests.Console
{
    /// <summary>
    /// TDD tests for Phase 4: Console Data Discovery endpoints (RED → GREEN → REFACTOR)
    /// </summary>
    public class ConsoleClientPhase4Tests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public ConsoleClientPhase4Tests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        }

        // ========================================
        // GET /console/data/categories (blob-bx-c) - 3 tests
        // ========================================

        [Fact]
        public async Task GetDataCategoriesAsync_ReturnsCategories()
        {
            // Arrange
            var json = JsonSerializer.Serialize(new { categories = new Dictionary<string, object> { { "test", "value" } } }, AppClient.JsonOptions);
            
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            var result = await client.GetDataCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Categories.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDataCategoriesAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Server error")
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.GetDataCategoriesAsync();

            // Assert
            await act.Should().ThrowAsync<TransportException>()
                .WithMessage("*HTTP request failed*")
                .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetDataCategoriesAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                () => client.GetDataCategoriesAsync());
            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // GET /console/data/paths (blob-bx-f) - 3 tests
        // ========================================

        [Fact]
        public async Task GetDataPathsAsync_ReturnsPaths()
        {
            // Arrange
            var json = JsonSerializer.Serialize(new { val = new[] { "test.path" }, child = new Dictionary<string, object>() }, AppClient.JsonOptions);
            
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            var result = await client.GetDataPathsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Val.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDataPathsAsync_HttpError_ThrowsTransportException()
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
                    Content = new StringContent("Not found")
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.GetDataPathsAsync();

            // Assert
            await act.Should().ThrowAsync<TransportException>()
                .WithMessage("*HTTP request failed*")
                .Where(ex => ex.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetDataPathsAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                () => client.GetDataPathsAsync());
            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // GET /console/data/paths/{path} (blob-bx-f with path param) - 3 tests
        // ========================================

        [Fact]
        public async Task GetDataPathsAsync_WithPath_ReturnsPaths()
        {
            // Arrange
            var json = JsonSerializer.Serialize(new { val = new[] { "sub.path" }, child = new Dictionary<string, object>() }, AppClient.JsonOptions);
            
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            var result = await client.GetDataPathsAsync("test.path");

            // Assert
            result.Should().NotBeNull();
            result.Val.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDataPathsAsync_WithPath_HttpError_ThrowsTransportException()
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

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.GetDataPathsAsync("test.path");

            // Assert
            await act.Should().ThrowAsync<TransportException>()
                .WithMessage("*HTTP request failed*")
                .Where(ex => ex.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetDataPathsAsync_WithPath_NetworkError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                () => client.GetDataPathsAsync("test.path"));
            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // GET /console/data/definitions/{path} (blob-bx-d) [DEPRECATED] - 3 tests
        // ========================================

#pragma warning disable CS0618 // Type or member is obsolete
        [Fact]
        public async Task GetDataDefinitionsAsync_ReturnsDefinitions()
        {
            // Arrange
            var json = JsonSerializer.Serialize(new { definitions = new Dictionary<string, object> { { "test", new object() } } }, AppClient.JsonOptions);
            
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            var result = await client.GetDataDefinitionsAsync("test.path");

            // Assert
            result.Should().NotBeNull();
            result.Definitions.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDataDefinitionsAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Server error")
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.GetDataDefinitionsAsync("test.path");

            // Assert
            await act.Should().ThrowAsync<TransportException>()
                .WithMessage("*HTTP request failed*")
                .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetDataDefinitionsAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                () => client.GetDataDefinitionsAsync("test.path"));
            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }
#pragma warning restore CS0618 // Type or member is obsolete

        // ========================================
        // GET /console/data/definitions2/{path} (blob-bx-e) - 3 tests
        // ========================================

        [Fact]
        public async Task GetDataDefinitions2Async_ReturnsDefinitions()
        {
            // Arrange
            var json = JsonSerializer.Serialize(new { node = new { defaultFilterType = 1 }, value = new { unit = "dB" } }, AppClient.JsonOptions);
            
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            var result = await client.GetDataDefinitions2Async("test.path");

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDataDefinitions2Async_HttpError_ThrowsTransportException()
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
                    Content = new StringContent("Not found")
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act
            Func<Task> act = async () => await client.GetDataDefinitions2Async("test.path");

            // Assert
            await act.Should().ThrowAsync<TransportException>()
                .WithMessage("*HTTP request failed*")
                .Where(ex => ex.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetDataDefinitions2Async_NetworkError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                () => client.GetDataDefinitions2Async("test.path"));
            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }

        // ========================================
        // POST /console/data/unsubscribe (blob-bw-j) - 3 tests
        // ========================================

        [Fact]
        public async Task PostDataUnsubscribeAsync_Success_ReturnsVoid()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);
            var request = new ConsoleDataSubscribeRequest { Path = "test.path", Format = "val" };

            // Act
            Func<Task> act = async () => await client.PostDataUnsubscribeAsync(request);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task PostDataUnsubscribeAsync_HttpError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Server error")
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);
            var request = new ConsoleDataSubscribeRequest { Path = "test.path", Format = "val" };

            // Act
            Func<Task> act = async () => await client.PostDataUnsubscribeAsync(request);

            // Assert
            await act.Should().ThrowAsync<TransportException>()
                .WithMessage("*HTTP request failed*")
                .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task PostDataUnsubscribeAsync_NetworkError_ThrowsTransportException()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost:8045/") };
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var client = new ConsoleClient(_httpClientFactoryMock.Object);
            var request = new ConsoleDataSubscribeRequest { Path = "test.path", Format = "val" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TransportException>(
                () => client.PostDataUnsubscribeAsync(request));
            exception.InnerException.Should().BeOfType<HttpRequestException>();
        }
    }
}
