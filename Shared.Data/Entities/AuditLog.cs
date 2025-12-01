using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Shared.Data.Entities
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        public long Id { get; set; }

        [MaxLength(100)]
        public required string UserId { get; set; }

        [MaxLength(50)]
        public required string Action { get; set; }

        [MaxLength(100)]
        public required string TableName { get; set; }

        public DateTime Timestamp { get; set; }

        public required string PrimaryKey { get; set; }

        // Use string to store JSON
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? AffectedColumns { get; set; }
    }
}