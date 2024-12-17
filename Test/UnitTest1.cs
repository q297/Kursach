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
    private const string TESTUSER = "John Doe2345";
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string? _token;

    public ApiTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _client = factory.CreateClient();
        _token = JwtHelper.CreateJwt1("John Doe21");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnOk_and_Token()
    {
        var user = new { Login = TESTUSER, Password = "petushok" };
        var response = await _client.PostAsJsonAsync("/api", user);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(token);
        jsonDocument.RootElement.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PatchPasswordAsync_ShouldReturnOk_and_Token()
    {
        var user = new
        {
            Login = TESTUSER, Password = "petushok",
            NewPassword = "newPassword1337_228!@#4"
        };

        var response = await _client.PatchAsJsonAsync("/api", user);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(token);
        _testOutputHelper.WriteLine(token);
        jsonDocument.RootElement.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PatchPasswordAsync_ShouldReturnUnauthorized()
    {
        var user = new
        {
            Login = TESTUSER, Password = "petushok123",
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
        var cipherSettings = new
        {
            RowCount = 4,
            SecretKey = "secretKey"
        };
        var response = await _client.PostAsJsonAsync("/api/cipher", "someencryptedtext");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var response2 = await _client.PostAsJsonAsync("/api/cipher/encrypt/1", cipherSettings);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        _testOutputHelper.WriteLine(response2.Content.ReadAsStringAsync().Result);
    }

    [Fact]
    public async Task GetTextAsync_And_DecryptTextAsync_ShouldBeAsExpected()
    {
        var cipherSettings = new
        {
            RowCount = 4,
            SecretKey = "secretKey"
        };
        var response = await _client.PostAsJsonAsync("/api/cipher/decrypt/1", cipherSettings);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var response1 = await _client.GetAsync("api/cipher/1");
        var decryptedText = await response1.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(decryptedText);
        jsonDocument.RootElement.GetString().Should().Be("someencryptedtext");
    }
}