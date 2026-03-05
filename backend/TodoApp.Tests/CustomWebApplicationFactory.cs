using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Api.Data;

public class CustomWebApplicationFactory
: WebApplicationFactory<Program>
{
  private SqliteConnection _connection = null!;

  protected override void ConfigureWebHost(
      Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
  {

    builder.UseEnvironment("Testing");

    builder.ConfigureServices(services =>
        {
          var descriptor = services.SingleOrDefault(
              d => d.ServiceType ==
              typeof(DbContextOptions<AppDbContext>));

          if (descriptor != null)
            services.Remove(descriptor);

          _connection = new SqliteConnection("DataSource=:memory:");
          _connection.Open();

          services.AddDbContext<AppDbContext>(options =>
              {
                options.UseSqlite(_connection);
              });

          var sp = services.BuildServiceProvider();
          using var scope = sp.CreateScope();
          var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
          db.Database.EnsureCreated();
        });
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    _connection?.Close();
  }
}
