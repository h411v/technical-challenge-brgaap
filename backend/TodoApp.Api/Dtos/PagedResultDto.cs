namespace TodoApp.Api.Dtos
{
    public class PagedResultDto<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public List<T> Data { get; set; } = new();
    }
}