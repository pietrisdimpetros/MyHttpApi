using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Shared.Audit.Models
{
    internal class AuditEntry
    {
        public required string UserId { get; set; }
        public required string Action { get; set; }
        public required string TableName { get; set; }
        public required DateTime Timestamp { get; set; }
        public Dictionary<string, object?> OldValues { get; } = [];
        public Dictionary<string, object?> NewValues { get; } = [];
        public List<string> AffectedColumns { get; } = [];
        public List<PropertyEntry> TemporaryProperties { get; } = [];

        public string? PrimaryKey { get; set; } // Set after save

        public string SerializeOldValues() => OldValues.Count > 0 ? JsonSerializer.Serialize(OldValues) : "{}";
        public string SerializeNewValues() => NewValues.Count > 0 ? JsonSerializer.Serialize(NewValues) : "{}";
        public string SerializeAffected() => AffectedColumns.Count > 0 ? JsonSerializer.Serialize(AffectedColumns) : "[]";
    }
}
