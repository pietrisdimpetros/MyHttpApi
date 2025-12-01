using System.Text.Json.Serialization;

namespace Shared.ExternalApiIntegration.Models
{
    internal sealed record OAuthTokenResponse(
       [property: JsonPropertyName("access_token")] string AccessToken,
       [property: JsonPropertyName("expires_in")] int ExpiresInSeconds,
       [property: JsonPropertyName("token_type")] string TokenType
   );
}