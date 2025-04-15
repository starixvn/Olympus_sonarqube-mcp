using System.Text.Json.Serialization;

namespace CursorMCP.Api.Models
{
    public class PaginationParameters
    {
        private int _pageSize = 20;
        private int _pageNumber = 1;

        [JsonPropertyName("page_number")]
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        [JsonPropertyName("page_size")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 20 : value > 100 ? 100 : value;
        }
    }

    public class PaginatedResponse<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("pagination")]
        public PaginationInfo Pagination { get; set; }

        public PaginatedResponse(T data, int totalItems, int pageNumber, int pageSize)
        {
            Data = data;
            Pagination = new PaginationInfo
            {
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }
    }

    public class PaginationInfo
    {
        [JsonPropertyName("total_items")]
        public int TotalItems { get; set; }

        [JsonPropertyName("page_number")]
        public int PageNumber { get; set; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("has_previous_page")]
        public bool HasPreviousPage { get; set; }

        [JsonPropertyName("has_next_page")]
        public bool HasNextPage { get; set; }
    }
} 