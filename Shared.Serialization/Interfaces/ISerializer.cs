namespace Shared.Serialization.Interfaces
{
    public interface ISerializer
    {
        string Serialize<T>(T value);
        T? Deserialize<T>(string value);

        // Async overloads are usually needed for Streams (Files/HTTP)
        Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken ct = default);
        Task SerializeAsync<T>(Stream stream, T value, CancellationToken ct = default);
    }
}