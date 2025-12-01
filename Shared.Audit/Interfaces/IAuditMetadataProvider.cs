namespace Shared.Audit.Interfaces
{
    public interface IAuditMetadataProvider
    {
        Task<string> GetUserIdAsync();
    }
}