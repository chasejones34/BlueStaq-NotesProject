using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TeamNotes.Api.Contracts;

namespace TeamNotes.Api.Tests;

public class ApiAuthorizationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiAuthorizationTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTeams_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/teams");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                usernameOrEmail = "does-not-exist",
                password = "WrongPassword123!"
            });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwt()
    {
        var uniqueId = Guid.NewGuid().ToString("N");

        var createUserResponse = await _client.PostAsJsonAsync(
            "/api/users",
            new
            {
                username = $"testuser{uniqueId}",
                email = $"testuser{uniqueId}@example.com",
                password = "Password123!"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createUserResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                usernameOrEmail = $"testuser{uniqueId}",
                password = "Password123!"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResult);
        Assert.False(
            string.IsNullOrWhiteSpace(loginResult!.AccessToken));
    }
}