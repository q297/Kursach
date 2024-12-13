using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Test;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _testOutputHelper;
    private string? _token;

    public ApiTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        // Используем фабрику для создания клиента
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnOk_and_Token()
    {
        var user = new { Login = "John Doe1", Password = "petushok" };

        var response = await _client.PostAsJsonAsync("/api", user);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(token);
        token.Should().NotBeNullOrEmpty();
        _testOutputHelper.WriteLine(token);
        _token = jsonDocument.RootElement.GetProperty("token").GetString();
    }

    [Fact]
    public async Task PatchPasswordAsync_ShouldReturnOk_and_Token()
    {
        var user = new
        {
            Login = "John Doe1", Password = "petushok",
            NewPassword = "newPassword1337_228!@#4"
        };

        var response = await _client.PatchAsJsonAsync("/api", user);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(token);
        token.Should().NotBeNullOrEmpty();
        _testOutputHelper.WriteLine(token);
        _token = jsonDocument.RootElement.GetProperty("token").GetString();
    }

    [Fact]
    public async Task PatchPasswordAsync_ShouldReturnUnauthorized()
    {
        var user = new
        {
            Login = "John Doe", Password = "petushok123",
            NewPassword = "newPassword1337_228!@#4"
        };

        var response = await _client.PatchAsJsonAsync("/api", user);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var token = await response.Content.ReadAsStringAsync();
        token.Should().NotBeNullOrEmpty();
        _testOutputHelper.WriteLine(token);
    }

    [Fact]
    public async Task GetUserHistoryAsync_ShouldReturnOk()
    {
        if (_token is null)
            await CreateUserAsync_ShouldReturnOk_and_Token();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var response = await _client.GetAsync("/api");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadAsStringAsync();
        history.Should().Contain("Регистрация");
        _testOutputHelper.WriteLine(history);
    }

    [Fact]
    public async Task AddTextAsync_And_EncryptTextAsync_ShouldReturnOk()
    {
        if (_token is null)
            await CreateUserAsync_ShouldReturnOk_and_Token();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var cipherSettings = new
        {
            RowCount = 3,
            SecretKey = "secretKey"
        };
        var response = await _client.PostAsJsonAsync("/api/cipher", "Hello World");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var response2 = await _client.PostAsJsonAsync("/api/cipher/encrypt/1", cipherSettings);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}