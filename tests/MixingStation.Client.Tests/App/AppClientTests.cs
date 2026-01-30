/**
 * Test Suite: AppClient HTTP Transport
 * 
 * SOURCE OF TRUTH: docs/openapi.json (http://localhost:8045/openapi.json)
 * Public API mirrors REST endpoints exactly (ADR-002: #11)
 * 
 * Verifies: #4 (REQ-F-001: HTTP Transport Layer)
 *           #5 (REQ-F-002: Read Application State)
 * Architecture: #10 (ADR-001: .NET 8 Runtime)
 *               #11 (ADR-002: REST Client Policy)
 * 
 * TDD Phase: RED (Write failing tests first - all tests fail with NotImplementedException)
 * 
 * Test Coverage:
 * - PostMixersConnectAsync: Valid request, invalid credentials, network errors, timeout
 * - GetMixersCurrentAsync: Success, not connected, network errors, malformed response
 * - Error handling: TransportException with HttpStatusCode
 * - HTTP client lifecycle: IHttpClientFactory integration
 * 
 * Reference: docs/openapi-spec-usage.md
 */

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using MixingStation.Client.App;
using MixingStation.Client.Exceptions;
using MixingStation.Client.Models;
using Xunit;

namespace MixingStation.Client.Tests.App;

public class AppClientTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;

    public AppClientTests()
    {
        // Setup mock HTTP message handler
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8045/")
        };

        // Setup mock HTTP client factory
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);
    }

    #region PostMixersConnectAsync Tests

    [Fact]
    public async Task PostMixersConnectAsync_ValidRequest_Completes()
    {
        // Arrange - OpenAPI: POST /app/mixers/connect
        // Request: blob-ca-b (consoleId, ip)
        // Response: 204 No Content (NO RESPONSE BODY!)
        var request = new AppMixersConnectRequest
        {
            ConsoleId = 1, // Mixer series ID (e.g., 1 = Behringer X32)
            Ip = "192.168.1.100"
        };

        SetupHttpNoContentResponse(HttpStatusCode.NoContent);

        var client = new AppClient(_httpClientFactoryMock.Object);

        // Act
        await client.PostMixersConnectAsync(request);

        // Assert - No exception thrown, verify HTTP request was made
        VerifyHttpRequest(HttpMethod.Post, "/app/mixers/connect");
    }

    [Fact]
    public async Task PostMixersConnectAsync_InvalidCredentials_ThrowsTransportException()
    {
        // Arrange
        var request = new AppMixersConnectRequest
        {
            ConsoleId = 1,
            Ip = "192.168.1.100"
        };

        SetupHttpNoContentResponse(HttpStatusCode.Unauthorized);

        var client = new AppClient(_httpClientFactoryMock.Object);

        // Act
        Func<Task> act = async () => await client.PostMixersConnectAsync(request);

        // Assert
        await act.Should().ThrowAsync<TransportException>()
            .WithMessage("*401*")
            .Where(ex => ex.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostMixersConnectAsync_NetworkError_ThrowsTransportException()
    {
        // Arrange
        var request = new AppMixersConnectRequest
        {
            ConsoleId = 1,
            Ip = "192.168.1.100"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var client = new AppClient(_httpClientFactoryMock.Object);

        // Act
        Func<Task> act = async () => await client.PostMixersConnectAsync(request);

        // Assert
        await act.Should().ThrowAsync<TransportException>()
            .WithMessage("*Network error*");
    }

    [Fact]
    public async Task PostMixersConnectAsync_Timeout_ThrowsTransportException()
    {
        // Arrange
        var request = new AppMixersConnectRequest
        {
            ConsoleId = 1,
            Ip = "192.168.1.100"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        var client = new AppClient(_httpClientFactoryMock.Object);

        // Act
        Func<Task> act = async () => await client.PostMixersConnectAsync(request);

        // Assert
        await act.Should().ThrowAsync<TransportException>()
            .WithMessage("*timeout*");
    }

    [Fact]
    public async Task PostMixersConnectAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var client = new AppClient(_httpClientFactoryMock.Object);

        // Act
        Func<Task> act = async () => await client.PostMixersConnectAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    #endregion

    #region GetMixersCurrentAsync Tests

    [Fact]
    public async Task GetMixersCurrentAsync_Success_ReturnsMixerInfo()
    {
        // Arrange - OpenAPI: GET /app/mixers/current
        // Response: blob-ca-d$b (11 properties from REAL API)
        var expectedResponse = new AppMixersCurrentResponse
        {
            ConsoleId = 1,
            CurrentModelId = 5,
            IpAddress = "192.168.1.100",
            Name = "Behringer X32", // Mixer series name
            FirmwareVersion = "4.06",
            CurrentModel = "X32-RACK",
            Manufacturer = "Behringer",
            Models = new[] { "X32", "X32-RACK", "X32-COMPACT" },
            SupportedHardwareModels = new[] { "X32", "X32-RACK" },
            CanSearch = true,
            ManufacturerId = 1
        };

        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        var client = new AppClient(_httpClientFactoryMock.Object);

        // Act
        var result = await client.GetMixersCurrentAsync();

        // Assert - Verify REAL properties from blob-ca-d$b
        result.Should().NotBeNull();
        result.ConsoleId.Should().Be(1);
        result.Name.Should().Be("Behringer X32");
        result.FirmwareVersion.Should().Be("4.06");
        result.CurrentModel.Should().Be("X32-RACK");
        result.Manufacturer.Should().Be("Behringer");
        result.IpAddress.Should().Be("192.168.1.100");

        VerifyHttpRequest(HttpMethod.Get, "/app/mixers/current");
    }

    [Fact]
    public async Task GetMixersCurrentAsync_NotConnected_ThrowsTransportException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, new { error = "Not connected" });

        var client = new AppClient(_httpClientFactoryMock.Object);

        // Act
        Func<Task> act = async () => await client.GetMixersCurrentAsync();

        // Assert
        await act.Should().ThrowAsync<TransportException>()
            .WithMessage("*400*")
            .Where(ex => ex.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMixersCurrentAsync_MalformedResponse_ThrowsTransportException()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.PathAndQuery.Contains("/app/mixers/current")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Invalid JSON {{{", Encoding.UTF8, "application/json")
            });

        var client = new AppClient(_httpClientFactoryMock.Object);

        // Act
        Func<Task> act = async () => await client.GetMixersCurrentAsync();

        // Assert
        await act.Should().ThrowAsync<TransportException>()
            .WithMessage("*JSON*");
    }

    [Fact]
    public async Task GetMixersCurrentAsync_ServerError_ThrowsTransportException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError, new { error = "Internal server error" });

        var client = new AppClient(_httpClientFactoryMock.Object);

        // Act
        Func<Task> act = async () => await client.GetMixersCurrentAsync();

        // Assert
        await act.Should().ThrowAsync<TransportException>()
            .WithMessage("*500*")
            .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError);
    }

    #endregion

    #region HTTP Client Lifecycle Tests

    [Fact]
    public void Constructor_NullHttpClientFactory_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new AppClient(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClientFactory");
    }

    [Fact]
    public async Task MultipleRequests_ReusesHttpClient()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, new AppMixersCurrentResponse
        {
            ConsoleId = 1,
            Name = "Behringer X32",
            FirmwareVersion = "4.06",
            CurrentModel = "X32",
            IpAddress = "192.168.1.100",
            Manufacturer = "Behringer",
            CurrentModelId = 5,
            Models = Array.Empty<string>(),
            SupportedHardwareModels = Array.Empty<string>(),
            CanSearch = true,
            ManufacturerId = 1
        });

        var client = new AppClient(_httpClientFactoryMock.Object);

        // Act
        await client.GetMixersCurrentAsync();
        await client.GetMixersCurrentAsync();

        // Assert
        _httpClientFactoryMock.Verify(
            f => f.CreateClient(It.IsAny<string>()),
            Times.Exactly(2), // Called once per request (per IHttpClientFactory design)
            "IHttpClientFactory should create client for each request");
    }

    #endregion

    #region Helper Methods

    private void SetupHttpResponse<T>(HttpStatusCode statusCode, T responseObject)
    {
        var jsonResponse = JsonSerializer.Serialize(responseObject);
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });
    }

    private void SetupHttpNoContentResponse(HttpStatusCode statusCode)
    {
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = statusCode
            // No Content for 204 No Content responses
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);
    }

    private void VerifyHttpRequest(HttpMethod method, string path)
    {
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri!.PathAndQuery.Contains(path)),
                ItExpr.IsAny<CancellationToken>());
    }

    #endregion
}
