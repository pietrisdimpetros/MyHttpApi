namespace Shared.Serialization.Exceptions
{
    public class PayloadSerializationException(string message, Exception innerException, string? content = null) : Exception(message, innerException)
    {
        public string? ContentPreview { get; } = content?.Length > 500 ? content[..500] + "..." : content;
    }
}