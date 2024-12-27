using Client.Models;
using RestSharp;
using RestSharp.Authenticators;
using Spectre.Console;

namespace Client.Controllers;

public class Controller
{
    public class ApiClient
    {
        private const string baseUrl = "http://localhost:5114/api";
        private readonly RestClient _httpClient;

        public ApiClient()
        {
            var options = new RestClientOptions(baseUrl)
            {
                AllowMultipleDefaultParametersWithSameName = false,
                Timeout = TimeSpan.FromSeconds(10)
            };
            _httpClient = new RestClient(options);
            _httpClient.AddDefaultHeader("Content-Type", "application/json");
        }

        private RestRequest PreparRequest(string value, string url = "")
        {
            var jwt = new JwtAuthenticator(value);
            var request = new RestRequest
            {
                Authenticator = jwt,
                Resource = baseUrl + url
            };
            return request;
        }

        public async Task<bool> RegisterOrLoginAsync(string url, User user)
        {
            var parameters = new
            {
                user.Login, user.Password
            };
            var request = new RestRequest(url).AddJsonBody(parameters);
            var response = await _httpClient.ExecuteAsync<User>(request, Method.Post);

            if (!response.IsSuccessful)
            {
                AnsiConsole.MarkupLineInterpolated(
                    $"[red]Произошла ошибка[/] {response.StatusCode} - {response.Content}");
                return false;
            }

            user.UserId = response.Data!.UserId;
            user.Jwt = response.Data!.Jwt;
            user.JwtExpiry = response.Data.JwtExpiry;

            return true;
        }

        public async Task<string> PatchPasswordAsync(object data)
        {
            var request = new RestRequest().AddJsonBody(data);
            var response = await _httpClient.ExecuteAsync<PatchPasswordResponse>(request, Method.Patch);

            if (!response.IsSuccessful)
                AnsiConsole.MarkupLineInterpolated(
                    $"[red]Произошла ошибка[/] {response.StatusCode} - {response.Content}");
            AnsiConsole.MarkupLine($"[green]Ответ сервера: {response.Data!.Message}[/]");
            return response.Data!.Token;
        }

        public async Task<IEnumerable<UserHistoryResponse>> GetRequestHistoryAsync(string userJwt)
        {
            var response =
                await _httpClient.ExecuteAsync<UserHistoryResponseWrapper>(PreparRequest(userJwt), Method.Get);
            if (!response.IsSuccessful)
            {
                AnsiConsole.MarkupLineInterpolated(
                    $"[red]Произошла ошибка:[/] {response.StatusCode} - {response.Content}");
                return Enumerable.Empty<UserHistoryResponse>();
            }

            if (response.Data == null || !response.Data.UserHistories.Any())
            {
                AnsiConsole.MarkupLine("[green]История запросов пуста[/]");
                return Enumerable.Empty<UserHistoryResponse>();
            }

            return response.Data.UserHistories;
        }

        public async Task PatchMessageAsync(string userJwt, int id, string message)
        {
            var request = PreparRequest(userJwt, "/cipher/" + id);
            request.AddJsonBody($"\"{message}\"");
            var response = await _httpClient.PatchAsync(request);
            if (!response.IsSuccessful)
                AnsiConsole.MarkupLineInterpolated(
                    $"[red]Произошла ошибка:[/] {response.StatusCode} - {response.Content}");
        }

        public async Task DeleteRequestHistoryAsync(string userJwt)
        {
            var response = await _httpClient.DeleteAsync(PreparRequest(userJwt));
            if (!response.IsSuccessful)
                AnsiConsole.MarkupLineInterpolated(
                    $"[red]Произошла ошибка:[/] {response.StatusCode} - {response.Content}");
            else
                AnsiConsole.MarkupLine("[green]История запросов очищена[/]");
        }

        public async Task EncryptTextAsync(string userJwt, int rowCount, string secretKey, int id)
        {
            var request = PreparRequest(userJwt, "/cipher/encrypt/" + id);
            var parameters = new
            {
                rowCount, secretKey
            };
            request.AddJsonBody(parameters);
            var response = await _httpClient.ExecuteAsync<string>(request, Method.Post);
            if (!response.IsSuccessful)
                AnsiConsole.MarkupLineInterpolated(
                    $"[red]Произошла ошибка:[/] {response.StatusCode} - {response.Content}");
            AnsiConsole.MarkupLine("[green]Текст зашифрован[/]");
        }

        public async Task DecryptTextAsync(string userJwt, string rowCount, string secretKey, int id)
        {
            var request = PreparRequest(userJwt, "/cipher/decrypt/" + id);
            var parameters = new
            {
                rowCount, secretKey
            };
            request.AddJsonBody(parameters);
            var response = await _httpClient.ExecuteAsync<object>(request, Method.Post);
            if (!response.IsSuccessful)
                AnsiConsole.MarkupLineInterpolated(
                    $"[red]Произошла ошибка:[/] {response.StatusCode} - {response.Content}");
            AnsiConsole.MarkupLine("[green]Текст расшифрован[/]");
        }

        public async Task<IEnumerable<MessagesResponse>> GetMessagesAsync(string userJwt)
        {
            var request = PreparRequest(userJwt, "/cipher/");
            var response = await _httpClient.ExecuteAsync<List<MessagesResponse>>(request, Method.Get);
            if (!response.IsSuccessful)
                AnsiConsole.MarkupLineInterpolated(
                    $"[red]Произошла ошибка:[/] {response.StatusCode} - {response.Content}");
            return response.Data!;
        }

        public async Task<string> GetMessageAsync(string userJwt, int id)
        {
            var request = PreparRequest(userJwt, "/cipher/" + id);
            var response = await _httpClient.GetAsync(request);
            if (!response.IsSuccessful)
                AnsiConsole.MarkupLineInterpolated(
                    $"[red]Произошла ошибка:[/] {response.StatusCode} - {response.Content}");
            return response.Content!;
        }

        public async Task AddMessageAsync(string userJwt, string message)
        {
            var request = PreparRequest(userJwt, "/cipher");
            request.AddJsonBody($"\"{message}\"");

            var response = await _httpClient.PostAsync(request);
            if (!response.IsSuccessful)
                AnsiConsole.MarkupLineInterpolated(
                    $"[red]Произошла ошибка:[/] {response.StatusCode} - {response.Content}");

            AnsiConsole.MarkupLineInterpolated(
                $"[green]Сообщение отправлено успешно.[/] [cornsilk1]Номер сообщения[/] {response.Content}");
        }

        public async Task DeleteMessageAsync(string userJwt, int messageId)
        {
            var request = PreparRequest(userJwt, "/cipher/" + messageId);
            var response = await _httpClient.DeleteAsync(request);
            if (!response.IsSuccessful)
                AnsiConsole.MarkupLineInterpolated(
                    $"[red]Произошла ошибка:[/] {response.StatusCode} - {response.Content}");
            else
                AnsiConsole.MarkupLine("[green]Сообщение успешно удалено[/]");
        }
    }
}