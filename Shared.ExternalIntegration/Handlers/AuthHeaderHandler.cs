using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.ExternalApiIntegration.Configuration;
using Shared.ExternalApiIntegration.Interfaces;
using System.Net.Http.Headers;

namespace Shared.ExternalApiIntegration.Handlers
{
    public sealed class AuthHeaderHandler(
      IMemoryCache memoryCache,
      IOptions<ExternalApiOptions> options,
      ITokenFetcher tokenFetcher,
      ILogger<AuthHeaderHandler> logger) : DelegatingHandler
    {
        private readonly ExternalApiOptions _config = options.Value;
        private const string CacheKey = "external_api_bearer_token";

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // 1. Add Configuration Header
            request.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", _config.OcpApimSubscriptionKey);

            // 2. Get Token (Cache Look-aside pattern)
            var token = await memoryCache.GetOrCreateAsync(CacheKey, async entry =>
            {
                logger.LogInformation("Cache Miss: Fetching new token from Identity Provider.");

                var (accessToken, validFor) = await tokenFetcher.GetNewTokenAsync(cancellationToken);

                // Set cache expiration
                entry.AbsoluteExpirationRelativeToNow = validFor;

                logger.LogInformation("Token acquired and cached. Expires in {Expiration}", validFor);

                return accessToken;
            });

            // 3. Attach Bearer
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            logger.LogDebug("Sending request to {Uri}", request.RequestUri);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
