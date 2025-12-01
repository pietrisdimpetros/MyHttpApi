using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.ExternalApiIntegration.Configuration;
using Shared.ExternalApiIntegration.Interfaces;
using Shared.ExternalApiIntegration.Models;
using System.Net.Http.Json;

namespace Shared.ExternalApiIntegration.Authentication
{

    public sealed class OidcTokenFetcher(
          IHttpClientFactory httpClientFactory,
          IOptions<IdentityProviderOptions> options,
          ILogger<OidcTokenFetcher> logger) : ITokenFetcher
    {
        private readonly IdentityProviderOptions _config = options.Value;

        public async Task<(string AccessToken, TimeSpan ValidFor)> GetNewTokenAsync(CancellationToken ct)
        {
            logger.LogInformation("Requesting new Client Credentials token from {TokenUrl}", _config.TokenUrl);

            // 1. Create a "Clean" Client (No AuthHandler attached)
            var client = httpClientFactory.CreateClient("IdPClient");

            // 2. Prepare Form Data (Standard OAuth2)
            var formData = new FormUrlEncodedContent(
            [
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _config.ClientId),
            new KeyValuePair<string, string>("client_secret", _config.ClientSecret),
            new KeyValuePair<string, string>("scope", _config.Scope)
        ]);

            // 3. Execute Request
            // Note: The "IdPClient" should have its own resilience (defined in DI)
            var response = await client.PostAsync(_config.TokenUrl, formData, ct);

            // Throw if IdP is down (5xx) or Creds are wrong (4xx)
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                logger.LogError("Identity Provider rejected request. Status: {Status}. Body: {Body}", response.StatusCode, errorContent);
                response.EnsureSuccessStatusCode();
            }

            // 4. Parse Response
            var result = await response.Content.ReadFromJsonAsync<OAuthTokenResponse>(cancellationToken: ct);

            if (result is null || string.IsNullOrWhiteSpace(result.AccessToken))
            {
                throw new InvalidOperationException("Identity Provider returned an empty access token.");
            }

            logger.LogInformation("Successfully acquired token. Expires in {Seconds} seconds.", result.ExpiresInSeconds);

            return (result.AccessToken, TimeSpan.FromSeconds(result.ExpiresInSeconds));
        }
    }
}
