using IdentityService.API.Data;
using IdentityService.API.DTOs;
using IdentityService.API.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<PagedResponse<AuditLogResponse>> GetAuditLogsAsync(AuditLogQueryParameters query)
        {
            if (query.PageNumber < 1)
            {
                query.PageNumber = 1;
            }

            if (query.PageSize < 1)
            {
                query.PageSize = 10;
            }

            var auditLogsQuery = _context.AuditLogs.AsQueryable();

            if (query.PerformedByUserId.HasValue)
            {
                auditLogsQuery = auditLogsQuery.Where(al =>
                    al.PerformedByUserId == query.PerformedByUserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Action))
            {
                var action = query.Action.Trim().ToLower();

                auditLogsQuery = auditLogsQuery.Where(al =>
                    al.Action.ToLower().Contains(action));
            }

            if (!string.IsNullOrWhiteSpace(query.EntityName))
            {
                var entityName = query.EntityName.Trim().ToLower();

                auditLogsQuery = auditLogsQuery.Where(al =>
                    al.EntityName.ToLower().Contains(entityName));
            }

            if (query.EntityId.HasValue)
            {
                auditLogsQuery = auditLogsQuery.Where(al =>
                    al.EntityId == query.EntityId.Value);
            }

            if (query.FromDate.HasValue)
            {
                auditLogsQuery = auditLogsQuery.Where(al =>
                    al.CreatedAt >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                auditLogsQuery = auditLogsQuery.Where(al =>
                    al.CreatedAt <= query.ToDate.Value);
            }

            var totalCount = await auditLogsQuery.CountAsync();

            var auditLogs = await auditLogsQuery
                .OrderByDescending(al => al.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var items = auditLogs.Select(al => new AuditLogResponse
            {
                Id = al.Id,
                PerformedByUserId = al.PerformedByUserId,
                Action = al.Action,
                EntityName = al.EntityName,
                EntityId = al.EntityId,
                OldValues = al.OldValues,
                NewValues = al.NewValues,
                Description = al.Description,
                CreatedAt = al.CreatedAt
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            return new PagedResponse<AuditLogResponse>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<AuditLogResponse?> GetAuditLogByIdAsync(Guid id)
        {
            var auditLog = await _context.AuditLogs
                .FirstOrDefaultAsync(al => al.Id == id);

            if (auditLog == null)
            {
                return null;
            }

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