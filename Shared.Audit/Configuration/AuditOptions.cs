namespace Shared.Audit.Configuration
{
    public sealed class AuditOptions
    {
        public const string SectionName = "Audit";

        public bool Enabled { get; set; } = true;

        // Tables we never want to audit (e.g., The AuditLog table itself!)
        public List<string> ExcludedTables { get; set; } = ["AuditLogs", "__EFMigrationsHistory"];
    }
}