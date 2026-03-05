using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoApp.Api.Dtos;

public class TodoIntegrationTests
: IClassFixture<CustomWebApplicationFactory>
{
  private readonly HttpClient _client;

  public TodoIntegrationTests(
      CustomWebApplicationFactory factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task Get_Todos_Should_Return_Paginated_Result()
  {
    await _client.PostAsync("/sync", null);

    var response = await _client.GetAsync("/todos?page=1&pageSize=5");

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var result = await response.Content
      .ReadFromJsonAsync<PagedResultDto<TodoDto>>();

    result.Should().NotBeNull();
    result!.Page.Should().Be(1);
    result.PageSize.Should().Be(5);
    result.Data.Count.Should().BeLessThanOrEqualTo(5);
  }

  [Fact]
  public async Task Get_Todos_Should_Filter_By_Title()
  {
    await _client.PostAsync("/sync", null);

    var response = await _client.GetAsync("/todos?title=delectus");

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var result = await response.Content
      .ReadFromJsonAsync<PagedResultDto<TodoDto>>();

    result.Should().NotBeNull();
    result!.Data.Should()
      .OnlyContain(t => t.Title.Contains("delectus"));
  }

  [Fact]
  public async Task Should_Not_Allow_More_Than_5_Incomplete_Tasks()
  {
    await _client.PostAsync("/sync", null);

    for (int i = 1; i <= 5; i++)
    {
      await _client.PutAsJsonAsync($"/todos/{i}", new
      {
        completed = false
      });
    }

    await _client.PutAsJsonAsync("/todos/6", new
    {
      completed = true
    });

    var response = await _client.PutAsJsonAsync("/todos/6", new
    {
      completed = false
    });

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("User 1 already has 5 incomplete tasks.");
  }

  [Fact]
  public async Task Put_Should_Update_Todo_Completed_Status()
  {
    await _client.PostAsync("/sync", null);

    var updateResponse = await _client.PutAsJsonAsync("/todos/1", new
    {
      completed = true
    });

    updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

    var response = await _client.GetAsync("/todos/1");

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var todo = await response.Content.ReadFromJsonAsync<TodoDto>();

    todo.Should().NotBeNull();
    todo!.Completed.Should().BeTrue();
  }
}
