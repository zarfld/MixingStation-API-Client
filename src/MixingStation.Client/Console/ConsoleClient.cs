/**
 * ConsoleClient - Implementation of IConsoleClient
 * 
 * REST client for /console/* endpoints (console state).
 * Public API mirrors REST endpoints exactly (ADR-002: #11).
 * 
 * Implements:
 * - #4 REQ-F-001: HTTP Client Transport
 * - #6 REQ-F-003: Console Data Reading
 * 
 * Architecture:
 * - #10 ADR-001: .NET 8 Runtime
 * - #11 ADR-002: HTTP Transport + REST Mirror Naming Policy
 * 
 * Design: Phase 1 Detailed Design (04-design/phase-1-detailed-design.md)
 * OpenAPI: docs/openapi.json
 * 
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/4
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/6
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/11
 */

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MixingStation.Client.App;
using MixingStation.Client.Exceptions;
using MixingStation.Client.Models;

namespace MixingStation.Client.Console
{
    /// <summary>
    /// REST client for /console/* endpoints (console state).
    /// Full implementation for TDD GREEN phase.
    /// </summary>
    public class ConsoleClient : IConsoleClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleClient"/> class.
        /// </summary>
        /// <param name="httpClientFactory">HTTP client factory.</param>
        /// <exception cref="ArgumentNullException">Thrown when httpClientFactory is null.</exception>
        public ConsoleClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <inheritdoc />
        public async Task<ConsoleInformationResponse> GetInformationAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                // GET /console/information
                var response = await httpClient.GetAsync("/console/information", cancellationToken)
                    .ConfigureAwait(false);

                // Check for HTTP errors
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken)
                        .ConfigureAwait(false);
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode}: {errorContent}",
                        response.StatusCode);
                }

                // Deserialize response
                var result = await response.Content.ReadFromJsonAsync<ConsoleInformationResponse>(
                    JsonOptions, cancellationToken).ConfigureAwait(false);

                if (result == null)
                {
                    throw new TransportException("Failed to deserialize response: received null");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"Failed to deserialize response: {ex.Message}", null, ex);
            }
            catch (TransportException)
            {
                // Re-throw TransportException as-is
                throw;
            }
            catch (OperationCanceledException)
            {
                // Re-throw cancellation as-is
                throw;
            }
            catch (Exception ex)
            {
                throw new TransportException($"Unexpected error: {ex.Message}", null, ex);
            }
        }

        /// <inheritdoc />
        public async Task<ConsoleDataValue> GetDataGetAsync(
            string path,
            string format,
            CancellationToken cancellationToken = default)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                // GET /console/data/get/{path}/{format}
                var response = await httpClient.GetAsync($"/console/data/get/{path}/{format}", cancellationToken)
                    .ConfigureAwait(false);

                // Check for HTTP errors
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken)
                        .ConfigureAwait(false);
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode}: {errorContent}",
                        response.StatusCode);
                }

                // Deserialize response
                var result = await response.Content.ReadFromJsonAsync<ConsoleDataValue>(
                    JsonOptions, cancellationToken).ConfigureAwait(false);

                if (result == null)
                {
                    throw new TransportException("Failed to deserialize response: received null");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"Failed to deserialize response: {ex.Message}", null, ex);
            }
            catch (TransportException)
            {
                // Re-throw TransportException as-is
                throw;
            }
            catch (OperationCanceledException)
            {
                // Re-throw cancellation as-is
                throw;
            }
            catch (Exception ex)
            {
                throw new TransportException($"Unexpected error: {ex.Message}", null, ex);
            }
        }

        /// <inheritdoc/>
        public async Task PostDataSubscribeAsync(
            ConsoleDataSubscribeRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            try
            {
                var httpClient = _httpClientFactory.CreateClient(nameof(ConsoleClient));
                var json = JsonSerializer.Serialize(request, AppClient.JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"));

                var response = await httpClient.PostAsync("/console/data/subscribe", content, cancellationToken)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    throw new TransportException(
                        $"HTTP request failed with status {response.StatusCode}: {errorBody}",
                        response.StatusCode);
                }

                // 204 No Content - nothing to return
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ConsoleDataValue> PostDataSetAsync(
            string path,
            string format,
            ConsoleDataValue request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(path, nameof(path));
            ArgumentNullException.ThrowIfNull(format, nameof(format));
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            try
            {
                var httpClient = _httpClientFactory.CreateClient(nameof(ConsoleClient));
                var json = JsonSerializer.Serialize(request, AppClient.JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"));

                var response = await httpClient.PostAsync($"/console/data/set/{path}/{format}", content, cancellationToken)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    throw new TransportException(
                        $"HTTP request failed with status {response.StatusCode}: {errorBody}",
                        response.StatusCode);
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    var result = JsonSerializer.Deserialize<ConsoleDataValue>(responseBody, AppClient.JsonOptions);
                    if (result == null)
                    {
                        throw new TransportException("Failed to deserialize response: result was null", response.StatusCode);
                    }
                    return result;
                }
                catch (JsonException ex)
                {
                    throw new TransportException(
                        $"Failed to deserialize response: {ex.Message}",
                        response.StatusCode,
                        ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        // ========================================
        // Phase 4: Console Data Discovery
        // ========================================

        /// <inheritdoc/>
        public async Task<ConsoleDataCategoriesResponse> GetDataCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(
                    $"/console/data/categories",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new TransportException(
                        $"HTTP request failed: {response.ReasonPhrase}. Response: {errorBody}",
                        response.StatusCode);
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    var result = JsonSerializer.Deserialize<ConsoleDataCategoriesResponse>(responseBody, AppClient.JsonOptions);
                    if (result == null)
                    {
                        throw new TransportException("Failed to deserialize response: result was null", response.StatusCode);
                    }
                    return result;
                }
                catch (JsonException ex)
                {
                    throw new TransportException(
                        $"Failed to deserialize response: {ex.Message}",
                        response.StatusCode,
                        ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ConsoleDataPathsResponse> GetDataPathsAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(
                    $"/console/data/paths",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new TransportException(
                        $"HTTP request failed: {response.ReasonPhrase}. Response: {errorBody}",
                        response.StatusCode);
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    var result = JsonSerializer.Deserialize<ConsoleDataPathsResponse>(responseBody, AppClient.JsonOptions);
                    if (result == null)
                    {
                        throw new TransportException("Failed to deserialize response: result was null", response.StatusCode);
                    }
                    return result;
                }
                catch (JsonException ex)
                {
                    throw new TransportException(
                        $"Failed to deserialize response: {ex.Message}",
                        response.StatusCode,
                        ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ConsoleDataPathsResponse> GetDataPathsAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(path, nameof(path));
            
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(
                    $"/console/data/paths/{Uri.EscapeDataString(path)}",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new TransportException(
                        $"HTTP request failed: {response.ReasonPhrase}. Response: {errorBody}",
                        response.StatusCode);
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    var result = JsonSerializer.Deserialize<ConsoleDataPathsResponse>(responseBody, AppClient.JsonOptions);
                    if (result == null)
                    {
                        throw new TransportException("Failed to deserialize response: result was null", response.StatusCode);
                    }
                    return result;
                }
                catch (JsonException ex)
                {
                    throw new TransportException(
                        $"Failed to deserialize response: {ex.Message}",
                        response.StatusCode,
                        ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        /// <inheritdoc/>
        [Obsolete("Use GetDataDefinitions2Async instead - /console/data/definitions2/{path}")]
        public async Task<ConsoleDataDefinitionsResponse> GetDataDefinitionsAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(path, nameof(path));
            
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(
                    $"/console/data/definitions/{Uri.EscapeDataString(path)}",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new TransportException(
                        $"HTTP request failed: {response.ReasonPhrase}. Response: {errorBody}",
                        response.StatusCode);
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    var result = JsonSerializer.Deserialize<ConsoleDataDefinitionsResponse>(responseBody, AppClient.JsonOptions);
                    if (result == null)
                    {
                        throw new TransportException("Failed to deserialize response: result was null", response.StatusCode);
                    }
                    return result;
                }
                catch (JsonException ex)
                {
                    throw new TransportException(
                        $"Failed to deserialize response: {ex.Message}",
                        response.StatusCode,
                        ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ConsoleDataDefinitions2Response> GetDataDefinitions2Async(
            string path,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(path, nameof(path));
            
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(
                    $"/console/data/definitions2/{Uri.EscapeDataString(path)}",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new TransportException(
                        $"HTTP request failed: {response.ReasonPhrase}. Response: {errorBody}",
                        response.StatusCode);
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    var result = JsonSerializer.Deserialize<ConsoleDataDefinitions2Response>(responseBody, AppClient.JsonOptions);
                    if (result == null)
                    {
                        throw new TransportException("Failed to deserialize response: result was null", response.StatusCode);
                    }
                    return result;
                }
                catch (JsonException ex)
                {
                    throw new TransportException(
                        $"Failed to deserialize response: {ex.Message}",
                        response.StatusCode,
                        ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        /// <inheritdoc/>
        public async Task PostDataUnsubscribeAsync(
            ConsoleDataSubscribeRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var json = JsonSerializer.Serialize(request, AppClient.JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(
                    "/console/data/unsubscribe",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new TransportException(
                        $"HTTP request failed: {response.ReasonPhrase}. Response: {errorBody}",
                        response.StatusCode);
                }

                // 204 No Content - nothing to return
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        // ========================================
        // Phase 5: Console Authentication & Mix Targets
        // ========================================

        /// <inheritdoc/>
        public async Task<ConsoleAuthInfoResponse> GetAuthInfoAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(
                    "/console/auth/info",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new TransportException(
                        $"HTTP request failed: {response.ReasonPhrase}. Response: {errorBody}",
                        response.StatusCode);
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    var result = JsonSerializer.Deserialize<ConsoleAuthInfoResponse>(responseBody, AppClient.JsonOptions);
                    if (result == null)
                    {
                        throw new TransportException("Failed to deserialize response: result was null", response.StatusCode);
                    }
                    return result;
                }
                catch (JsonException ex)
                {
                    throw new TransportException(
                        $"Failed to deserialize response: {ex.Message}",
                        response.StatusCode,
                        ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ConsoleAuthLoginResponse> PostAuthLoginAsync(
            ConsoleAuthLoginRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var json = JsonSerializer.Serialize(request, AppClient.JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(
                    "/console/auth/login",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new TransportException(
                        $"HTTP request failed: {response.ReasonPhrase}. Response: {errorBody}",
                        response.StatusCode);
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    var result = JsonSerializer.Deserialize<ConsoleAuthLoginResponse>(responseBody, AppClient.JsonOptions);
                    if (result == null)
                    {
                        throw new TransportException("Failed to deserialize response: result was null", response.StatusCode);
                    }
                    return result;
                }
                catch (JsonException ex)
                {
                    throw new TransportException(
                        $"Failed to deserialize response: {ex.Message}",
                        response.StatusCode,
                        ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ConsoleMixTargetsResponse> GetMixTargetsAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(
                    "/console/mixTargets",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new TransportException(
                        $"HTTP request failed: {response.ReasonPhrase}. Response: {errorBody}",
                        response.StatusCode);
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    var result = JsonSerializer.Deserialize<ConsoleMixTargetsResponse>(responseBody, AppClient.JsonOptions);
                    if (result == null)
                    {
                        throw new TransportException("Failed to deserialize response: result was null", response.StatusCode);
                    }
                    return result;
                }
                catch (JsonException ex)
                {
                    throw new TransportException(
                        $"Failed to deserialize response: {ex.Message}",
                        response.StatusCode,
                        ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        // ========================================
        // Phase 6: Console Metering
        // ========================================

        /// <inheritdoc/>
        [Obsolete("Use PostMetering2SubscribeAsync instead - /console/metering2/subscribe")]
        public async Task PostMeteringSubscribeAsync(
            ConsoleMeteringSubscribeRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var json = JsonSerializer.Serialize(request, AppClient.JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(
                    "/console/metering/subscribe",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new TransportException(
                        $"HTTP request failed: {response.ReasonPhrase}. Response: {errorBody}",
                        response.StatusCode);
                }

                // 204 No Content - nothing to return
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        /// <inheritdoc/>
        public async Task PostMeteringUnsubscribeAsync(
            ConsoleMeteringUnsubscribeRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var json = JsonSerializer.Serialize(request, AppClient.JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(
                    "/console/metering/unsubscribe",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new TransportException(
                        $"HTTP request failed: {response.ReasonPhrase}. Response: {errorBody}",
                        response.StatusCode);
                }

                // 204 No Content - nothing to return
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        /// <inheritdoc/>
        public async Task PostMetering2SubscribeAsync(
            ConsoleMeteringSubscribe2Request request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var json = JsonSerializer.Serialize(request, AppClient.JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(
                    "/console/metering2/subscribe",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new TransportException(
                        $"HTTP request failed: {response.ReasonPhrase}. Response: {errorBody}",
                        response.StatusCode);
                }

                // 204 No Content - nothing to return
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException("Network error during HTTP request", null, ex);
            }
        }

        // ========================================
        // Phase 10: Console Config Events
        // ========================================

        /// <inheritdoc/>
        public async Task GetOnConfigChangedAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync("/console/onConfigChanged", cancellationToken);

                // Expect 204 No Content (WebSocket event endpoint in REST mode)
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from GET /console/onConfigChanged (expected 204 No Content)",
                        response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (TransportException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new TransportException($"Unexpected error: {ex.Message}", null, ex);
            }
        }
    }
}
