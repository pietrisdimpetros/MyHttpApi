using Microsoft.Extensions.Logging;
using Shared.Serialization.Exceptions;
using Shared.Serialization.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Serialization.Services
{
    public sealed class SystemTextJsonSerializer(ILogger<SystemTextJsonSerializer> logger) : ISerializer
    {
        // 1. The "Golden" Configuration
        // This is the ONE place in your entire company where JSON rules exist.
        public static readonly JsonSerializerOptions DefaultOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false, // Save bandwidth in Prod
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters ={
                new JsonStringEnumConverter()
            }
        };
        public string Serialize<T>(T value)
        {
            try
            {
                return JsonSerializer.Serialize(value, DefaultOptions);
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "Failed to serialize object of type {Type}", typeof(T).Name);

                throw new PayloadSerializationException($"Error serializing {typeof(T).Name}", ex);
            }
        }

        public T? Deserialize<T>(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value)) return default;
                return JsonSerializer.Deserialize<T>(value, DefaultOptions);
            }
            catch (Exception ex)
            {
                //only log if logging level is enabled
                if (logger.IsEnabled(LogLevel.Error))
                {
                    var snippet = value.Length <= 512 ? value : string.Concat(value.AsSpan(0, 512), "...");
                    logger.LogError("Deserialization failure content snippet: {Snippet}", snippet);
                }

                // Re-throw with the content snippet for the developer
                throw new PayloadSerializationException($"Error deserializing {typeof(T).Name}", ex, value);
            }
        }

        public async Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken ct = default)
        {
            try
            {
                if (stream.CanSeek && stream.Length == 0) return default;
                return await JsonSerializer.DeserializeAsync<T>(stream, DefaultOptions, ct);
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "Failed to deserialize stream to {Type}", typeof(T).Name);
                throw new PayloadSerializationException($"Error deserializing stream to {typeof(T).Name}", ex);
            }
        }

        public async Task SerializeAsync<T>(Stream stream, T value, CancellationToken ct = default)
        {
            try
            {
                await JsonSerializer.SerializeAsync(stream, value, DefaultOptions, ct);
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "Failed to serialize {Type} into stream", typeof(T).Name);
                throw new PayloadSerializationException($"Error serializing {typeof(T).Name} to stream", ex);
            }
        }
    }
}
