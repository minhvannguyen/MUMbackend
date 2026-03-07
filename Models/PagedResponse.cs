namespace MUMbackend.Models
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public PagedResponse(IEnumerable<T> items, int currentPage, int pageSize, int totalItems)
        {
            Items = items;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalItems = totalItems;
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        }
    }
}