using IdentityService.API.DTOs;
using IdentityService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<AuditLogResponse>>>> GetAuditLogs(
            [FromQuery] AuditLogQueryParameters query)
        {
            var auditLogs = await _auditLogService.GetAuditLogsAsync(query);

            return Ok(ApiResponse<PagedResponse<AuditLogResponse>>.Ok(
                auditLogs,
                "Audit logs loaded successfully."
            ));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<AuditLogResponse>>> GetAuditLogById(Guid id)
        {
            var auditLog = await _auditLogService.GetAuditLogByIdAsync(id);

            if (auditLog == null)
            {
                return NotFound(ApiResponse<AuditLogResponse>.Fail("Audit log not found."));
            }

            return Ok(ApiResponse<AuditLogResponse>.Ok(
                auditLog,
                "Audit log loaded successfully."
            ));
        }
    }
}