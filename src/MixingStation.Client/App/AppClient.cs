/**
 * AppClient - HTTP transport implementation for /app/* endpoints
 * 
 * SOURCE OF TRUTH: docs/openapi.json (http://localhost:8045/openapi.json)
 * Public API mirrors REST endpoints exactly (ADR-002: #11)
 * 
 * Implements:
 * - #4 REQ-F-001: HTTP Client Transport
 * - #5 REQ-F-002: Application State Reading
 * 
 * Architecture:
 * - #10 ADR-001: .NET 8 Runtime
 * - #11 ADR-002: HTTP Transport + REST Mirror Naming Policy
 * 
 * Design: Phase 1 Detailed Design (04-design/phase-1-detailed-design.md)
 * Reference: docs/openapi-spec-usage.md
 * 
 * TDD Status: GREEN Phase - Implementation complete
 * 
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/4
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/5
 * See: https://github.com/zarfld/MixingStation-API-Client/issues/11
 */

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MixingStation.Client.Exceptions;
using MixingStation.Client.Models;

namespace MixingStation.Client.App
{
    /// <summary>
    /// HTTP transport implementation for /app/* endpoints.
    /// TDD GREEN Phase: Full implementation with error handling.
    /// </summary>
    public class AppClient : IAppClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        /// <summary>
        /// JSON serialization options for API requests/responses.
        /// Uses camelCase property naming to match REST API.
        /// </summary>
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public AppClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <inheritdoc />
        public async Task PostMixersConnectAsync(
            AppMixersConnectRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.PostAsJsonAsync(
                    "/app/mixers/connect",
                    request,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/mixers/connect",
                        response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new TransportException($"Request timeout: {ex.Message}", null, ex);
            }
            catch (TransportException)
            {
                // Re-throw TransportException as-is
                throw;
            }
            catch (Exception ex)
            {
                throw new TransportException($"Unexpected error: {ex.Message}", null, ex);
            }
        }

        /// <inheritdoc />
        public async Task<AppMixersCurrentResponse> GetMixersCurrentAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(
                    "/app/mixers/current",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from GET /app/mixers/current",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AppMixersCurrentResponse>(
                    cancellationToken: cancellationToken);

                if (result == null)
                {
                    throw new TransportException("Failed to deserialize response: null result");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new TransportException($"Request timeout: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"JSON deserialization error: {ex.Message}", null, ex);
            }
            catch (TransportException)
            {
                // Re-throw TransportException as-is
                throw;
            }
            catch (Exception ex)
            {
                throw new TransportException($"Unexpected error: {ex.Message}", null, ex);
            }
        }

        /// <inheritdoc />
        public async Task<AppStateResponse> GetStateAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                // GET /app/state
                var response = await httpClient.GetAsync("/app/state", cancellationToken)
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
                var result = await response.Content.ReadFromJsonAsync<AppStateResponse>(
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

        // ========================================
        // Phase 3: Mixer Lifecycle Management
        // ========================================

        /// <inheritdoc />
        public async Task<AppMixersAvailableResponse> GetMixersAvailableAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(
                    "/app/mixers/available",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from GET /app/mixers/available",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AppMixersAvailableResponse>(
                    JsonOptions,
                    cancellationToken);

                if (result == null)
                {
                    throw new TransportException("Failed to deserialize response: null result");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new TransportException($"Request timeout: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"JSON deserialization error: {ex.Message}", null, ex);
            }
            catch (TransportException)
            {
                // Re-throw TransportException as-is
                throw;
            }
            catch (Exception ex)
            {
                throw new TransportException($"Unexpected error: {ex.Message}", null, ex);
            }
        }

        /// <inheritdoc />
        public async Task PostMixersSearchAsync(
            AppMixersSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var json = JsonSerializer.Serialize(request, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8,
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"));

                var response = await httpClient.PostAsync(
                    "/app/mixers/search",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/mixers/search",
                        response.StatusCode);
                }

                // 204 No Content - success, no response body
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"Failed to serialize request: {ex.Message}", null, ex);
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
        public async Task<AppMixersSearchResultsResponse> GetMixersSearchResultsAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(
                    "/app/mixers/searchResults",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from GET /app/mixers/searchResults",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AppMixersSearchResultsResponse>(
                    JsonOptions,
                    cancellationToken);

                if (result == null)
                {
                    throw new TransportException("Failed to deserialize response: null result");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new TransportException($"Request timeout: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"JSON deserialization error: {ex.Message}", null, ex);
            }
            catch (TransportException)
            {
                // Re-throw TransportException as-is
                throw;
            }
            catch (Exception ex)
            {
                throw new TransportException($"Unexpected error: {ex.Message}", null, ex);
            }
        }

        /// <inheritdoc />
        public async Task PostMixersDisconnectAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.PostAsync(
                    "/app/mixers/disconnect",
                    null,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/mixers/disconnect",
                        response.StatusCode);
                }

                // 204 No Content - success, no response body
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
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
        public async Task PostMixersOfflineAsync(
            AppMixersOfflineRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var json = JsonSerializer.Serialize(request, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8,
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"));

                var response = await httpClient.PostAsync(
                    "/app/mixers/offline",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/mixers/offline",
                        response.StatusCode);
                }

                // 204 No Content - success, no response body
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"Failed to serialize request: {ex.Message}", null, ex);
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

        // ========================================
        // Phase 7: App Presets Management
        // ========================================

        /// <inheritdoc />
        public async Task<AppPresetsScopesResponse> GetPresetsScopesAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync("/app/presets/scopes", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from GET /app/presets/scopes",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AppPresetsScopesResponse>(
                    JsonOptions,
                    cancellationToken);

                return result ?? throw new TransportException(
                    "Received null response from GET /app/presets/scopes",
                    response.StatusCode);
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

        /// <inheritdoc />
        public async Task PostPresetsChannelApplyAsync(
            AppPresetsChannelApplyRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var json = JsonSerializer.Serialize(request, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8,
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"));

                var response = await httpClient.PostAsync(
                    "/app/presets/channel/apply",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/presets/channel/apply",
                        response.StatusCode);
                }

                // 204 No Content - success, no response body
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"Failed to serialize request: {ex.Message}", null, ex);
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

        /// <inheritdoc />
        public async Task<AppPresetsChannelCreateResponse> PostPresetsChannelCreateAsync(
            AppPresetsChannelCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var json = JsonSerializer.Serialize(request, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8,
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"));

                var response = await httpClient.PostAsync(
                    "/app/presets/channel/create",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/presets/channel/create",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AppPresetsChannelCreateResponse>(
                    JsonOptions,
                    cancellationToken);

                return result ?? throw new TransportException(
                    "Received null response from POST /app/presets/channel/create",
                    response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"Failed to serialize/deserialize: {ex.Message}", null, ex);
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

        /// <inheritdoc />
        public async Task PostPresetsScenesApplyAsync(
            AppPresetsSceneData request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var json = JsonSerializer.Serialize(request, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8,
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"));

                var response = await httpClient.PostAsync(
                    "/app/presets/scenes/apply",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/presets/scenes/apply",
                        response.StatusCode);
                }

                // 204 No Content - success, no response body
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"Failed to serialize request: {ex.Message}", null, ex);
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

        /// <inheritdoc />
        public async Task<AppPresetsSceneData> PostPresetsScenesCreateAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                // POST with no body (creates scene from current state)
                var response = await httpClient.PostAsync(
                    "/app/presets/scenes/create",
                    new StringContent(string.Empty),
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/presets/scenes/create",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AppPresetsSceneData>(
                    JsonOptions,
                    cancellationToken);

                return result ?? throw new TransportException(
                    "Received null response from POST /app/presets/scenes/create",
                    response.StatusCode);
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

        /// <inheritdoc />
        public async Task<AppPresetsLastErrorResponse> GetPresetsLastErrorAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync("/app/presets/lastError", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from GET /app/presets/lastError",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AppPresetsLastErrorResponse>(
                    JsonOptions,
                    cancellationToken);

                return result ?? throw new TransportException(
                    "Received null response from GET /app/presets/lastError",
                    response.StatusCode);
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

        // ========================================
        // Phase 8: App IDCA & UI Management
        // ========================================

        /// <inheritdoc />
        public async Task<AppIdcaResponse> PostIdcasAsync(
            AppIdcaRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var json = JsonSerializer.Serialize(request, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8,
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"));

                var response = await httpClient.PostAsync(
                    "/app/idcas",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/idcas",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AppIdcaResponse>(
                    JsonOptions,
                    cancellationToken);

                return result ?? throw new TransportException(
                    "Received null response from POST /app/idcas",
                    response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"Failed to serialize/deserialize: {ex.Message}", null, ex);
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

        /// <inheritdoc />
        public async Task<AppIdcaResponse> PostIdcasAsync(
            string index,
            AppIdcaRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(index))
            {
                throw new ArgumentException("Index cannot be null or empty", nameof(index));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var json = JsonSerializer.Serialize(request, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8,
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"));

                var response = await httpClient.PostAsync(
                    $"/app/idcas/{index}",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/idcas/{index}",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AppIdcaResponse>(
                    JsonOptions,
                    cancellationToken);

                return result ?? throw new TransportException(
                    $"Received null response from POST /app/idcas/{index}",
                    response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"Failed to serialize/deserialize: {ex.Message}", null, ex);
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

        /// <inheritdoc />
        public async Task PostIdcasDeleteAsync(
            string index,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(index))
            {
                throw new ArgumentException("Index cannot be null or empty", nameof(index));
            }

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.PostAsync(
                    $"/app/idcas/{index}/delete",
                    new StringContent(string.Empty),
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/idcas/{index}/delete",
                        response.StatusCode);
                }

                // 204 No Content - success, no response body
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

        /// <inheritdoc />
        public async Task<AppIdcaRearrangeResponse> PostIdcasRearrangeAsync(
            AppIdcaRearrangeRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var json = JsonSerializer.Serialize(request, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8,
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json"));

                var response = await httpClient.PostAsync(
                    "/app/idcas/rearrange",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/idcas/rearrange",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AppIdcaRearrangeResponse>(
                    JsonOptions,
                    cancellationToken);

                return result ?? throw new TransportException(
                    "Received null response from POST /app/idcas/rearrange",
                    response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new TransportException($"Network error: {ex.Message}", null, ex);
            }
            catch (JsonException ex)
            {
                throw new TransportException($"Failed to serialize/deserialize: {ex.Message}", null, ex);
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

        /// <inheritdoc />
        public async Task<AppUiSelectedChannelResponse> GetUiSelectedChannelAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync("/app/ui/selectedChannel", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from GET /app/ui/selectedChannel",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AppUiSelectedChannelResponse>(
                    JsonOptions,
                    cancellationToken);

                return result ?? throw new TransportException(
                    "Received null response from GET /app/ui/selectedChannel",
                    response.StatusCode);
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

        /// <inheritdoc />
        public async Task<AppUiSelectedChannelResponse> GetUiSelectedChannelAsync(
            string nameOrIndex,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nameOrIndex))
            {
                throw new ArgumentException("Name or index cannot be null or empty", nameof(nameOrIndex));
            }

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync($"/app/ui/selectedChannel/{nameOrIndex}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from GET /app/ui/selectedChannel/{nameOrIndex}",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AppUiSelectedChannelResponse>(
                    JsonOptions,
                    cancellationToken);

                return result ?? throw new TransportException(
                    $"Received null response from GET /app/ui/selectedChannel/{nameOrIndex}",
                    response.StatusCode);
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

        // ========================================
        // Phase 9: App Network & Misc
        // ========================================

        /// <inheritdoc/>
        public async Task<NetworkInterfacesResponse> GetNetworkInterfacesAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync("/app/network/interfaces", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from GET /app/network/interfaces",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<NetworkInterfacesResponse>(
                    JsonOptions,
                    cancellationToken);

                return result ?? throw new TransportException(
                    "Received null response from GET /app/network/interfaces",
                    response.StatusCode);
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

        /// <inheritdoc/>
        public async Task<NetworkInterfacesResponse> PostNetworkInterfacesPrimaryAsync(
            NetworkInterfacePrimaryRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var jsonContent = JsonSerializer.Serialize(request, JsonOptions);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                var response = await httpClient.PostAsync("/app/network/interfaces/primary", content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/network/interfaces/primary",
                        response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<NetworkInterfacesResponse>(
                    JsonOptions,
                    cancellationToken);

                return result ?? throw new TransportException(
                    "Received null response from POST /app/network/interfaces/primary",
                    response.StatusCode);
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

        /// <inheritdoc/>
        public async Task PostSaveAsync(
            CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                // POST /app/save requires empty body
                var content = new StringContent(string.Empty, System.Text.Encoding.UTF8);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                var response = await httpClient.PostAsync("/app/save", content, cancellationToken);

                // Expect 204 No Content
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new TransportException(
                        $"HTTP {(int)response.StatusCode} error from POST /app/save (expected 204 No Content)",
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
