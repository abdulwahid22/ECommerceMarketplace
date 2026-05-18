using IdentityService.API.Data;
using IdentityService.API.Models;

namespace IdentityService.API.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;

        public AuditLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            Guid? performedByUserId,
            string action,
            string entityName,
            Guid? entityId,
            string? oldValues = null,
            string? newValues = null,
            string? description = null)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                PerformedByUserId = performedByUserId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();
        }
    }
}