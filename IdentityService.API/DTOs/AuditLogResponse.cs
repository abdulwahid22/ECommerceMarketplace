namespace IdentityService.API.DTOs
{
    public class AuditLogResponse
    {
        public Guid Id { get; set; }

        public Guid? PerformedByUserId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public Guid? EntityId { get; set; }

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}