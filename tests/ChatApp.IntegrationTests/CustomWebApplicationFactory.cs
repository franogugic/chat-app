using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost; // Dodaj ovo!
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ChatApp.Infrastructure.Db;
using System.Linq;

namespace ChatApp.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestChatDb");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        });
    }
}