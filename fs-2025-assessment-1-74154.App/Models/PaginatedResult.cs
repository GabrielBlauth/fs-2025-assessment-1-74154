namespace fs_2025_assessment_1_74154_App.Models
{
    public class PaginatedResult<T>
    {
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
        public List<T> Items { get; set; } = new();
    }
}
