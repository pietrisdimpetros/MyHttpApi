namespace Shared.Caching.Interfaces
{
    public interface ICacheAgent
    {
        Task<T?> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? expiration = null,
            CancellationToken ct = default);

        Task RemoveAsync(string key, CancellationToken ct = default);
    }
}