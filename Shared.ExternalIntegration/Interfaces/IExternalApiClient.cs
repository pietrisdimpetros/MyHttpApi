namespace Shared.ExternalApiIntegration.Interfaces
{
    public interface IExternalApiClient
    {
        // Generic method to fetch typed data
        Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken ct = default);

        // Fallback for raw content
        Task<string> GetRawAsync(string endpoint, CancellationToken ct = default);
    }
}