using TodoApp.Api.Dtos;

namespace TodoApp.Api.Services
{
    public interface ITodoService
    {
        Task<PagedResultDto<TodoDto>> GetTodosAsync(
        int page,
        int pageSize,
        string? title,
        string? sort,
        string? order);

        Task<TodoDto?> GetByIdAsync(int id);

        Task<(bool Success, string? Error)> UpdateCompletedAsync(int id, bool completed);

        Task SyncAsync();
    }
}