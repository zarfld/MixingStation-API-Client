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
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
    }
}
