using IdentityService.API.DTOs;
using IdentityService.API.Models;

namespace IdentityService.API.Helpers
{
    public static class AuditLogMappingHelper
    {
        public static AuditLogResponse ToAuditLogResponse(AuditLog auditLog)
        {
            return new AuditLogResponse
            {
                Id = auditLog.Id,
                PerformedByUserId = auditLog.PerformedByUserId,
                Action = auditLog.Action,
                EntityName = auditLog.EntityName,
                EntityId = auditLog.EntityId,
                OldValues = auditLog.OldValues,
                NewValues = auditLog.NewValues,
                Description = auditLog.Description,
                CreatedAt = auditLog.CreatedAt
            };
        }
    }
}