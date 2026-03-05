using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Data;
using TodoApp.Api.Dtos;
using TodoApp.Api.Models;

namespace TodoApp.Api.Services
{
    public class TodoService : ITodoService
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _factory;

        public TodoService(AppDbContext db, IHttpClientFactory factory)
        {
            _db = db;
            _factory = factory;
        }

        public async Task<PagedResultDto<TodoDto>> GetTodosAsync(
            int page,
            int pageSize,
            string? title,
            string? sort,
            string? order)
        {
            var query = _db.Todos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(t => t.Title.Contains(title));

            query = (sort?.ToLower()) switch
            {
                "title" => order == "desc"
                    ? query.OrderByDescending(t => t.Title)
                    : query.OrderBy(t => t.Title),

                _ => order == "desc"
                    ? query.OrderByDescending(t => t.Id)
                    : query.OrderBy(t => t.Id)
            };

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TodoDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Title = t.Title,
                    Completed = t.Completed
                })
                .ToListAsync();

            return new PagedResultDto<TodoDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Data = items
            };
        }

        public async Task<TodoDto?> GetByIdAsync(int id)
        {
            return await _db.Todos.AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TodoDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Title = t.Title,
                    Completed = t.Completed
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Success, string? Error)> UpdateCompletedAsync(int id, bool completed)
        {
            var todo = await _db.Todos.FindAsync(id);

            if (todo is null)
                return (false, "User not found.");

            if (!completed && todo.Completed == true)
            {
                var incompleteCount = await _db.Todos
                    .CountAsync(t => t.UserId == todo.UserId && !t.Completed);

                if (incompleteCount >= 5)
                    return (false, $"User {todo.UserId} already has 5 incomplete tasks.");
            }

            todo.Completed = completed;
            await _db.SaveChangesAsync();

            return (true, null);
        }

        public async Task SyncAsync()
        {
            var client = _factory.CreateClient();
            string response = await client.GetStringAsync(
                "https://jsonplaceholder.typicode.com/todos")
                    ?? throw new HttpRequestException("The API response was null.");

            var todos = JsonSerializer.Deserialize<List<TodoItem>>(response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("Failed to deserialize data from external API.");

            var existingIds = await _db.Todos
                .Select(t => t.Id)
                .ToListAsync();

            var newTodos = todos
                .Where(t => !existingIds.Contains(t.Id))
                .ToList();

            _db.Todos.AddRange(newTodos);
            await _db.SaveChangesAsync();
        }
    }
}
