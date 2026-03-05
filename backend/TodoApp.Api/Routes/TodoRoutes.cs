using TodoApp.Api.Dtos;
using TodoApp.Api.Services;

namespace TodoApp.Api.Routes
{
    public static class TodoRoutes
    {
        public static void MapTodoRoutes(this WebApplication app)
        {
            app.MapGet("/todos", async (
                ITodoService service,
                int page = 1,
                int pageSize = 10,
                string? title = null,
                string? sort = "id",
                string? order = "asc") =>
            {
                var result = await service.GetTodosAsync(page, pageSize, title, sort, order);
                return Results.Ok(result);
            });

            app.MapGet("/todos/{id:int}", async (
                int id,
                ITodoService service) =>
            {
                var todo = await service.GetByIdAsync(id);

                return todo is null
                    ? Results.NotFound(new { message = "Task not found." })
                    : Results.Ok(todo);
            });

            app.MapPut("/todos/{id:int}", async (
                int id,
                UpdateCompleteDto dto,
                ITodoService service) =>
            {
                var result = await service.UpdateCompletedAsync(id, dto.Completed);

                if (!result.Success)
                    return Results.BadRequest(new { message = result.Error });

                return Results.Ok(new { message = "Task updated!" });
            });

            app.MapPost("/sync", async (ITodoService service) =>
            {
                await service.SyncAsync();
                return Results.Ok(new { message = "Data synchronized successfully." });
            });
        }
    }
}