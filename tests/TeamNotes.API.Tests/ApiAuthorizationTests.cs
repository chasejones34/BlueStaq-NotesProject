using System.Net;
using System.Net.Http.Headers;
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

    [Fact]
    public async Task CreateTeam_WithValidJwt_ReturnsCreated()
    {
        var uniqueId = Guid.NewGuid().ToString("N");

        var createUserResponse = await _client.PostAsJsonAsync(
            "/api/users",
            new
            {
                username = $"teamuser{uniqueId}",
                email = $"teamuser{uniqueId}@example.com",
                password = "Password123!"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createUserResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                usernameOrEmail = $"teamuser{uniqueId}",
                password = "Password123!"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResult);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult!.AccessToken);

        var createTeamResponse = await _client.PostAsJsonAsync(
            "/api/teams",
            new
            {
                name = $"Test Team {uniqueId}"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createTeamResponse.StatusCode);
    }

    [Fact]
    public async Task CreateAndRetrieveNote_WithValidJwt_ReturnsNote()
    {
        var uniqueId = Guid.NewGuid().ToString("N");

        var createUserResponse = await _client.PostAsJsonAsync(
            "/api/users",
            new
            {
                username = $"noteuser{uniqueId}",
                email = $"noteuser{uniqueId}@example.com",
                password = "Password123!"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createUserResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                usernameOrEmail = $"noteuser{uniqueId}",
                password = "Password123!"
            });

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginResult);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult!.AccessToken);

        var createTeamResponse = await _client.PostAsJsonAsync(
            "/api/teams",
            new
            {
                name = $"Note Team {uniqueId}"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createTeamResponse.StatusCode);

        var team =
            await createTeamResponse.Content
                .ReadFromJsonAsync<TeamResponse>();

        Assert.NotNull(team);

        var createNoteResponse = await _client.PostAsJsonAsync(
            $"/api/teams/{team!.Id}/notes",
            new
            {
                title = "Integration test note",
                content = "This note was created by an automated test."
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createNoteResponse.StatusCode);

        var note =
            await createNoteResponse.Content
                .ReadFromJsonAsync<NoteResponse>();

        Assert.NotNull(note);

        var getNoteResponse = await _client.GetAsync(
            $"/api/notes/{note!.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getNoteResponse.StatusCode);

        var retrievedNote =
            await getNoteResponse.Content
                .ReadFromJsonAsync<NoteResponse>();

        Assert.NotNull(retrievedNote);
        Assert.Equal(
            "Integration test note",
            retrievedNote!.Title);
    }

    [Fact]
    public async Task Editor_CannotDeleteNote_ReturnsForbidden()
    {
        var uniqueId = Guid.NewGuid().ToString("N");

        var ownerUsername = $"owner{uniqueId}";
        var editorUsername = $"editor{uniqueId}";

        await _client.PostAsJsonAsync(
            "/api/users",
            new
            {
                username = ownerUsername,
                email = $"{ownerUsername}@example.com",
                password = "Password123!"
            });

        var ownerLoginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                usernameOrEmail = ownerUsername,
                password = "Password123!"
            });

        var ownerLogin =
            await ownerLoginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(ownerLogin);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                ownerLogin!.AccessToken);

        var createTeamResponse = await _client.PostAsJsonAsync(
            "/api/teams",
            new
            {
                name = $"Permission Team {uniqueId}"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createTeamResponse.StatusCode);

        var team =
            await createTeamResponse.Content
                .ReadFromJsonAsync<TeamResponse>();

        Assert.NotNull(team);

        await _client.PostAsJsonAsync(
            "/api/users",
            new
            {
                username = editorUsername,
                email = $"{editorUsername}@example.com",
                password = "Password123!"
            });

        var addMemberResponse = await _client.PostAsJsonAsync(
            $"/api/teams/{team!.Id}/members",
            new
            {
                username = editorUsername,
                role = 2
            });

        Assert.Equal(
            HttpStatusCode.Created,
            addMemberResponse.StatusCode);

        var editorLoginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                usernameOrEmail = editorUsername,
                password = "Password123!"
            });

        var editorLogin =
            await editorLoginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(editorLogin);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                editorLogin!.AccessToken);

        var createNoteResponse = await _client.PostAsJsonAsync(
            $"/api/teams/{team.Id}/notes",
            new
            {
                title = "Editor note",
                content = "An Editor can create this note."
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createNoteResponse.StatusCode);

        var note =
            await createNoteResponse.Content
                .ReadFromJsonAsync<NoteResponse>();

        Assert.NotNull(note);

        var deleteResponse = await _client.DeleteAsync(
            $"/api/notes/{note!.Id}");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            deleteResponse.StatusCode);
    }
}