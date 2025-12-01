using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Caching.Configuration;
using Shared.Caching.Interfaces;
using System.Text.Json;

namespace Shared.Caching.Services
{
    public sealed class InMemoryCacheAgent(
     IMemoryCache memoryCache,
     IOptions<CacheOptions> options,
     ILogger<InMemoryCacheAgent> logger) : ICacheAgent
    {
        private readonly CacheOptions _config = options.Value;

        // Strict rules: Case insensitive, no indentation (compact)
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public async Task<T?> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? expiration = null,
            CancellationToken ct = default)
        {
            // 1. Enforce Naming Prefix
            string cacheKey = $"{_config.InstanceName}{key}";

            // 2. Try Get (Centralized Error Handling)
            try
            {
                if (memoryCache.TryGetValue(cacheKey, out string? cachedJson))
                {
                    if (!string.IsNullOrEmpty(cachedJson))
                    {
                        logger.LogDebug("Cache HIT (Memory) for key: {Key}", cacheKey);
                        // Deserialize to ensure we return a fresh object copy 
                        return JsonSerializer.Deserialize<T>(cachedJson, _jsonOptions);
                    }
                }
            }
            catch (Exception ex)
            {
                // Fail-Safe: If JSON is corrupt or cache fails, log and move on.
                logger.LogError(ex, "Memory Cache read error for key {Key}", cacheKey);
            }

            // 3. Cache Miss - Execute Factory
            logger.LogDebug("Cache MISS (Memory) for key: {Key}", cacheKey);
            var result = await factory(ct);

            if (result is null) return default;

            // 4. Set (Strict Serialization)
            try
            {
                var serialized = JsonSerializer.Serialize(result, _jsonOptions);

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_config.DefaultExpirationMinutes),
                    // Optimization: Enforce size limit if needed in future
                    Size = serialized.Length
                };

                memoryCache.Set(cacheKey, serialized, cacheEntryOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Memory Cache write error for key {Key}", cacheKey);
            }

            return result;
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            string cacheKey = $"{_config.InstanceName}{key}";
            memoryCache.Remove(cacheKey);
            return Task.CompletedTask;
        }
    }
}
