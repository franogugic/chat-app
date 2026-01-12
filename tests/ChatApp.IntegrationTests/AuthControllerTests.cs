using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChatApp.Application.DTO_s;
using FluentAssertions;
using Xunit;

namespace ChatApp.IntegrationTests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(Guid UserId, string Token)> RegisterAndLoginAsync(string email, string password)
    {
        var regRequest = new CreateUserRequestDTO 
        { 
            Name = "Test User", 
            Mail = email, 
            Password = password, 
            PhoneNumber = "091234567" 
        };
        var regRes = await _client.PostAsJsonAsync("/api/auth/register", regRequest);
        var regData = await regRes.Content.ReadFromJsonAsync<CreateUserResponseDTO>();

        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginUserRequestDTO 
        { 
            Mail = email, 
            Password = password 
        });

        var loginData = await loginRes.Content.ReadFromJsonAsync<AuthResponseDTO>();

        if (loginData == null || string.IsNullOrEmpty(loginData.AccessToken))
        {
            throw new Exception($"Login failed. Status: {loginRes.StatusCode}.");
        }

        return (regData!.Id, loginData.AccessToken);
    }

    [Fact]
    public async Task Register_ShouldReturnCreated_WhenDataIsValid()
    {
        var request = new CreateUserRequestDTO { Name = "Frano", Mail = $"frano.{Guid.NewGuid()}@test.com", Password = "Password123!", PhoneNumber = "091" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        var email = $"double.{Guid.NewGuid()}@test.com";
        var request = new CreateUserRequestDTO { Name = "F1", Mail = email, Password = "Pass123!", PhoneNumber = "1" };
        
        await _client.PostAsJsonAsync("/api/auth/register", request);
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ShouldReturnValidToken_WhenCredentialsAreValid()
    {
        var mail = $"jwt.{Guid.NewGuid()}@test.com";
        var pass = "Password123!";
        var (_, token) = await RegisterAndLoginAsync(mail, pass);

        token.Should().NotBeNullOrEmpty();
        token.Should().Contain(".");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginUserRequestDTO { Mail = "hacker@test.com", Password = "any" });
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _client.GetAsync($"/api/auth/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnForbidden_WhenAccessingOtherUser()
    {
        var userA = await RegisterAndLoginAsync($"usera.{Guid.NewGuid()}@test.com", "Password123!");
        var userB = await RegisterAndLoginAsync($"userb.{Guid.NewGuid()}@test.com", "Password123!");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userA.Token);
        var response = await _client.GetAsync($"/api/auth/{userB.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        var user = await RegisterAndLoginAsync($"empty.{Guid.NewGuid()}@test.com", "Password123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

        var response = await _client.GetAsync($"/api/auth/{Guid.Empty}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}