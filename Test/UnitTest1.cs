using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Test;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _testOutputHelper;

    public ApiTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        // Используем фабрику для создания клиента
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnOk_and_Token()
    {
        var user = new { Login = "John Doe", Password = "petushok" };

        // Отправка POST запроса
        var response = await _client.PostAsJsonAsync("/api", user);

        // Проверка статуса ответа
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Чтение и проверка токена
        var token = await response.Content.ReadAsStringAsync();
        token.Should().NotBeNullOrEmpty();
        _testOutputHelper.WriteLine(token);
    }

    [Fact]
    public async Task PatchPasswordAsync_ShouldReturnOk_and_Token()
    {
        var user = new
        {
            Login = "John Doe", Password = "petushok",
            NewPassword = "newPassword1337_228!@#4"
        };

        var response = await _client.PatchAsJsonAsync("/api", user);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadAsStringAsync();
        token.Should().NotBeNullOrEmpty();
        _testOutputHelper.WriteLine(token);
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
}