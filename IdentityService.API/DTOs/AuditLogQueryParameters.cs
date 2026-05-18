namespace IdentityService.API.DTOs
{
    public class AuditLogQueryParameters
    {
        private const int MaxPageSize = 100;

        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public Guid? PerformedByUserId { get; set; }

        public string? Action { get; set; }

        public string? EntityName { get; set; }

        public Guid? EntityId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}