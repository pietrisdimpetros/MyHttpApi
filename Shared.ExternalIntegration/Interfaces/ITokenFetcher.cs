namespace Shared.ExternalApiIntegration.Interfaces
{
    public interface ITokenFetcher
    {
        Task<(string AccessToken, TimeSpan ValidFor)> GetNewTokenAsync(CancellationToken ct);
    }
}
