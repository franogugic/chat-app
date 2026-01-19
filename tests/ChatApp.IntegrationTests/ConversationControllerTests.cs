using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using ChatApp.Infrastructure.Db;
using ChatApp.Domain.Entities;
using Xunit;

namespace ChatApp.IntegrationTests;

public class ConversationControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ConversationControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            });
        }).CreateClient();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
    }

    [Fact]
    public async Task GetPrivateConversation_ShouldReturnNoContent_WhenConversationDoesNotExist()
    {
        var otherUser = TestUserFactory.CreateUser();
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(otherUser);
            await db.SaveChangesAsync();
        }

        // ACT
        var response = await _client.GetAsync($"/api/conversation/private/{otherUser.Id}");

        // ASSERT
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CreatePrivateConversation_ShouldReturnOk_WhenSuccessful()
    {
        var otherUser = TestUserFactory.CreateUser("Other", "other@test.com");
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(otherUser);
            await db.SaveChangesAsync();
        }

        // ACT
        var response = await _client.PostAsJsonAsync("/api/conversation/private/create", otherUser.Id);

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreatePrivateConversation_WithSelf_ReturnsBadRequest()
    {
        var currentUserId = Guid.Parse(TestAuthHandler.UserId);

        // ACT
        var response = await _client.PostAsJsonAsync("/api/conversation/private/create", currentUserId);

        // ASSERT
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string UserId = "00000000-0000-0000-0000-000000000001";

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { 
            new Claim(ClaimTypes.NameIdentifier, UserId),
            new Claim(ClaimTypes.Name, "TestUser") 
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}