using IdentityService.API.DTOs;

namespace IdentityService.API.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(
            Guid? performedByUserId,
            string action,
            string entityName,
            Guid? entityId,
            string? oldValues = null,
            string? newValues = null,
            string? description = null
        );

        Task<PagedResponse<AuditLogResponse>> GetAuditLogsAsync(AuditLogQueryParameters query);

        Task<AuditLogResponse?> GetAuditLogByIdAsync(Guid id);
    }
}