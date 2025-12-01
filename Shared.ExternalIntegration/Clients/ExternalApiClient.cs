using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.ExternalApiIntegration.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace Shared.ExternalApiIntegration.Clients
{
    public sealed class ExternalApiClient(
    HttpClient httpClient,
    IOptions<JsonSerializerOptions> jsonOptions,
    ILogger<ExternalApiClient> logger) : IExternalApiClient
    {
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value;
        public async Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken ct = default)
        {
            try
            {
                if (logger.IsEnabled(LogLevel.Debug))
                    logger.LogDebug("Initiating GET request to {Endpoint}", endpoint);

                // 1. Send the Request
                // The BaseAddress and Auth Headers are already attached by the DI container and Handlers
                using var response = await httpClient.GetAsync(endpoint, ct);

                // 2. The Trigger Mechanism
                // This is CRITICAL. If the API returns 500, 502, 503, or 408, 
                // this throws an HttpRequestException.
                // The Resilience Pipeline catches this specific exception and triggers Retries/Circuit Breakers.
                response.EnsureSuccessStatusCode();

                // 3. Deserialize if successful
                // If we reached here, the circuit is closed and the API is healthy.
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return default;
                }

                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, ct);
            }
            catch (HttpRequestException ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "HTTP Request failed after resilience policies applied. StatusCode: {StatusCode}", ex.StatusCode);
                throw; // Re-throw so the Global Exception Handler (ProblemDetails) picks it up
            }
        }

        public async Task<string> GetRawAsync(string endpoint, CancellationToken ct = default)
        {
            var response = await httpClient.GetAsync(endpoint, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(ct);
        }
    }
}
