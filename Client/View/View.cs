using Client.Controllers;
using Client.Models;
using Spectre.Console;

namespace Client.View;

internal class View
{
    private readonly Controller.ApiClient _client = new();
    private Settings _settings;
    private User _user;

    private async Task<bool> RegisterLogin()
    {
        var url = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Выберите действие:")
            .AddChoices("Зарегистрироваться", "Войти", "Выход из программы"));

        if (url == "Выход из программы") Environment.Exit(0);

        url = url == "Зарегистрироваться" ? "register" : "login";

        _user.Login = AnsiConsole.Prompt(new TextPrompt<string>("Введите логин:"));
        _user.Password = AnsiConsole.Prompt(new TextPrompt<string>("Введите пароль:").Secret());

        return await _client.RegisterOrLoginAsync(url, _user);
    }

    private async Task EnsureUserAuthenticated()
    {
        while (_user.Login == null || string.IsNullOrEmpty(_user.Jwt))
        {
            AnsiConsole.MarkupLine(_user.Login == null
                ? "[red]Данные о пользователе не найдены.[/]"
                : "[red]Токен не найден или просрочен.[/]");

            if (!await RegisterLogin())
                AnsiConsole.MarkupLine("[red]Ошибка входа. Попробуйте снова.[/]");
            else
                UserManager.SaveUserData(_user);
        }
    }

    public async Task MainAsync()
    {
        SettingsManager.SettingsChanged += () => { _settings = SettingsManager.LoadSettings(); };
        UserManager.UserDataChanged += () => { UserManager.PrintUserData(_user); };
        _settings = SettingsManager.LoadSettings();
        _user = UserManager.LoadUserData();


        await EnsureUserAuthenticated();

        while (true)
        {
            var moduleSelection = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Выберите модуль:")
                .AddChoices("Модуль пользователя", "Модуль шифрования", "Перейти в регистрацию/логин",
                    "Выход из программы"));

            switch (moduleSelection)
            {
                case "Модуль пользователя":
                    await UserModule();
                    break;

                case "Модуль шифрования":
                    await CipherModule();
                    break;

                case "Перейти в регистрацию/логин":
                    await RegisterLogin();
                    UserManager.SaveUserData(_user);
                    break;

                case "Выход из программы":
                    Environment.Exit(0);
                    break;
            }
        }
    }

    private async Task UserModule()
    {
        var userAction = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Модуль пользователя")
            .AddChoices("Изменить пароль", "Посмотреть историю запросов", "Удалить историю запросов", "Назад"));

        switch (userAction)
        {
            case "Изменить пароль":
                await ChangePassword();
                break;

            case "Посмотреть историю запросов":
                await ShowRequestHistory();
                break;

            case "Удалить историю запросов":
                await DeleteRequestHistory();
                break;
        }
    }

    private async Task ChangePassword()
    {
        var oldPassword = AnsiConsole.Prompt(new TextPrompt<string>("Введите старый пароль:").Secret());
        var newPassword = AnsiConsole.Prompt(new TextPrompt<string>("Введите новый пароль:").Secret());
        var confirmPassword = AnsiConsole.Prompt(new TextPrompt<string>("Подтвердите новый пароль:").Secret());

        if (newPassword == confirmPassword)
        {
            _user.Jwt = await _client.PatchPasswordAsync(new
                { _user.Login, Password = oldPassword, NewPassword = newPassword });
            AnsiConsole.MarkupLine("[green]Пароль успешно изменён![/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Пароли не совпадают![/]");
            await ChangePassword();
        }
    }

    private async Task ShowRequestHistory()
    {
        var history = await _client.GetRequestHistoryAsync(_user.Jwt);
        var table = new Table()
            .BorderColor(Color.Aquamarine3)
            .AddColumn("Логин")
            .AddColumn("Тип запроса")
            .AddColumn("Детали запроса");

        foreach (var entry in history) table.AddRow(entry.Login, entry.QueryType, entry.QueryDetails);

        AnsiConsole.Write(table);
    }

    private async Task DeleteRequestHistory()
    {
        await _client.DeleteRequestHistoryAsync(_user.Jwt);
        AnsiConsole.MarkupLine("[green]История запросов успешно удалена![/]");
    }

    private async Task CipherModule()
    {
        var cipherAction = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Модуль шифрования")
            .AddChoices("Зашифровать", "Расшифровать", "Добавить сообщение", "Посмотреть сообщения",
                "Получить сообщение", "Настройки шифрования", "Назад"));

        switch (cipherAction)
        {
            case "Зашифровать":
                await EncryptMessage();
                break;

            case "Расшифровать":
                await DecryptMessage();
                break;

            case "Добавить сообщение":
                await AddMessage();
                break;

            case "Посмотреть сообщения":
                await ViewMessages();
                break;

            case "Получить сообщение":
                await GetMessage();
                break;

            case "Настройки шифрования":
                UpdateEncryptionSettings();
                break;
        }
    }

    private async Task EncryptMessage()
    {
        var useSavedSettings = AnsiConsole.Confirm("Использовать сохранённые настройки?");

        var encryptionRowCount = useSavedSettings
            ? _settings.RowCount
            : AnsiConsole.Ask<int>("Введите количество строк для шифрования:");
        var encryptionKey = useSavedSettings
            ? _settings.SecretKey
            : AnsiConsole.Ask<string>("Введите ключ для шифрования:");
        var encryptionId = AnsiConsole.Ask<int>("Введите номер сообщения:");

        await _client.EncryptTextAsync(_user.Jwt, encryptionRowCount, encryptionKey, encryptionId);
        AnsiConsole.MarkupLine(await _client.GetMessageAsync(_user.Jwt, encryptionId));
    }

    private async Task DecryptMessage()
    {
        var useSavedSettings = AnsiConsole.Confirm("Использовать сохранённые настройки?");

        var decryptionRowCount = useSavedSettings
            ? _settings.RowCount
            : AnsiConsole.Ask<int>("Введите количество строк для расшифрования:");
        var decryptionKey = useSavedSettings
            ? _settings.SecretKey
            : AnsiConsole.Ask<string>("Введите ключ для расшифрования:");
        var decryptionId = AnsiConsole.Ask<int>("Введите номер сообщения:");

        await _client.DecryptTextAsync(_user.Jwt, decryptionRowCount.ToString(), decryptionKey, decryptionId);
        AnsiConsole.MarkupLine(await _client.GetMessageAsync(_user.Jwt, decryptionId));
    }

    private async Task AddMessage()
    {
        var message = AnsiConsole.Prompt(new TextPrompt<string>("Введите сообщение:"));
        await _client.AddMessageAsync(_user.Jwt, message);
    }

    private async Task ViewMessages()
    {
        var messages = await _client.GetMessagesAsync(_user.Jwt);
        var table = new Table()
            .AddColumn("Номер")
            .AddColumn("Сообщение");

        foreach (var entry in messages) table.AddRow(entry.MessageNumber.ToString(), entry.Text);

        AnsiConsole.Write(table);
    }

    private async Task GetMessage()
    {
        var messageId = AnsiConsole.Ask<int>("Введите номер сообщения:");
        var message = await _client.GetMessageAsync(_user.Jwt, messageId);
        AnsiConsole.MarkupLine($"[green]Сообщение:[/] {message}");
    }

    private void UpdateEncryptionSettings()
    {
        AnsiConsole.MarkupLine("[yellow]Текущие настройки:[/]");
        AnsiConsole.MarkupLine($"[blue]Количество строк:[/] {_settings.RowCount}");
        AnsiConsole.MarkupLine($"[blue]Ключ:[/] {_settings.SecretKey}");

        var newRowCount = AnsiConsole.Ask("Введите новое количество строк (или оставьте текущее значение):",
            _settings.RowCount);
        var newSecretKey = AnsiConsole.Ask("Введите новый ключ (или оставьте текущий):", _settings.SecretKey);

        var newSettings = new Settings { RowCount = newRowCount, SecretKey = newSecretKey };
        SettingsManager.SaveSettings(newSettings);
        AnsiConsole.MarkupLine("[green]Настройки успешно обновлены![/]");
    }
}