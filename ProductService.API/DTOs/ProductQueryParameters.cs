namespace ProductService.API.DTOs
{
    public class ProductQueryParameters
    {
        private const int MaxPageSize = 100;

        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string? Search { get; set; }

        public Guid? CategoryId { get; set; }

        public Guid? SellerId { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public bool? IsActive { get; set; }

        public string? Language { get; set; } = "en";
    }
}