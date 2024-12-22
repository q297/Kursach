using Client.Controllers;
using Client.Models;
using Spectre.Console;

namespace Client.View;

internal class View
{
    private readonly Controller.ApiClient _client = new();
    private readonly UserManager _userManager = new();
    private Settings _settings;
    private User _user;

    private async Task<bool> RegisterLogin()
    {
        var url = string.Empty;
        var result = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Выберите, что хотите сделать.")
            .AddChoices("[green]Зарегистрироваться[/]", "[green]Войти[/]", "[red3]Выход из программы[/]"));
        switch (result)
        {
            case "[green]Зарегистрироваться[/]":
                url = "register";
                break;
            case "[green]Войти[/]":
                url = "login";
                break;
            case "[red3]Выход из программы[/]":
                Environment.Exit(0);
                break;
        }

        _user.Login = AnsiConsole.Prompt(new TextPrompt<string>("Введите логин: "));
        _user.Password = AnsiConsole.Prompt(new TextPrompt<string>("Введите пароль: ").Secret());
        return await _client.RegisterOrLoginAsync(url, _user);
    }

    public async Task MainAsync()
    {
        SettingsManager.SettingsChanged += () => { _settings = SettingsManager.LoadSettings(); };
        _settings = SettingsManager.LoadSettings();
        _user = _userManager.LoadUserData();
        var cipherModule = new SelectionPrompt<string>().Title("Модуль шифрования")
            .AddChoices("Зашифровать", "Расшифровать", "Получить сообщение", "Добавить сообщение",
                "Посмотреть сообщения", "Настройки шифрования", "Выход из программы").EnableSearch()
            .MoreChoicesText("[green]Используйте стрелки для просмотра[/]");
        var userModule = new SelectionPrompt<string>().Title("Модуль пользователя").AddChoices("Изменить пароль",
                "Посмотреть историю запросов",
                "Удалить историю запросов", "Выход из программы").EnableSearch()
            .MoreChoicesText("[green]Используйте стрелки для просмотра[/]");
        var textPrompt =
            new TextPrompt<bool>("[orange1]Выберите модуль:[/] Модуль пользователя - 1, Модуль шифрования - 2")
                .AddChoice(true).AddChoice(false).WithConverter(choice => choice ? "1" : "2");
        if (_user.Login == null
            || _user.Jwt == string.Empty)
        {
            AnsiConsole.MarkupLine(_user.Login == null
                ? "[red]Данные об пользователе не найдены.[/]"
                : "[red]Токен не найден. Скорее всего он просрочен.[/]");
            while (!await RegisterLogin())
            {
            }

            _userManager.SaveUserData(_user);
        }

        _userManager.PrintUserData(_user);
        while (true)
        {
            var confirmation = AnsiConsole.Prompt(textPrompt);

            switch (confirmation)
            {
                case true:
                    var userResult = AnsiConsole.Prompt(userModule);

                    switch (userResult)
                    {
                        case "Изменить пароль":
                            var oldPassword =
                                AnsiConsole.Prompt(new TextPrompt<string>("Введите старый пароль: ").Secret());
                            var newPassword =
                                AnsiConsole.Prompt(new TextPrompt<string>("Введите новый пароль: ").Secret());
                            var confirmNewPassword =
                                AnsiConsole.Prompt(new TextPrompt<string>("Подтвердите новый пароль: ").Secret());

                            if (newPassword == confirmNewPassword)
                            {
                                var token = await _client.PatchPasswordAsync(new
                                    { _user.Login, Password = oldPassword, NewPassword = newPassword });
                                AnsiConsole.MarkupLine("[green]Пароль успешно изменён![/]");
                                _user.Jwt = token;
                            }
                            else
                            {
                                AnsiConsole.MarkupLine("[red]Пароли не совпадают![/]");
                                goto case "Изменить пароль";
                            }

                            break;

                        case "Посмотреть историю запросов":
                            var history = await _client.GetRequestHistoryAsync(_user.Jwt);
                            AnsiConsole.MarkupLine("[blue]История запросов:[/]");
                            var table = new Table()
                                .BorderColor(Color.Green)
                                .AddColumn("Логин")
                                .AddColumn("Тип запроса")
                                .AddColumn("Детали запроса");
                            foreach (var entry in history)
                                table.AddRow(entry.Login, entry.QueryType, entry.QueryDetails);
                            AnsiConsole.Write(table);
                            break;

                        case "Удалить историю запросов":
                            await _client.DeleteRequestHistoryAsync(_user.Jwt);
                            AnsiConsole.MarkupLine("[green]История запросов успешно удалена![/]");
                            break;

                        case "Выход из программы":
                            Environment.Exit(0);
                            break;
                    }

                    break;

                case false:
                    var cipherResult = AnsiConsole.Prompt(cipherModule);

                    switch (cipherResult)
                    {
                        case "Зашифровать":
                            var useSavedSettings = await AnsiConsole.ConfirmAsync("Использовать сохраненные настройки?");

                            int encryptionRowCount;
                            string encryptionKey;
                            if (useSavedSettings)
                            {
                                encryptionRowCount = _settings.RowCount;
                                encryptionKey = _settings.SecretKey;
                                AnsiConsole.MarkupLine("[yellow]Используются сохраненные настройки:[/]");
                                AnsiConsole.MarkupLine($"[blue]Количество строк:[/] {encryptionRowCount}");
                                AnsiConsole.MarkupLine($"[blue]Ключ:[/] {encryptionKey}");
                            }
                            else
                            {
                                encryptionRowCount = AnsiConsole.Ask<int>("Введите количество строк для шифрования:");
                                encryptionKey = AnsiConsole.Ask<string>("Введите ключ для шифрования:");
                            }
                            var encryptionId = AnsiConsole.Ask<int>("Введите номер сообщения:");
                            await _client.EncryptTextAsync(_user.Jwt, encryptionRowCount, encryptionKey, encryptionId);
                            AnsiConsole.MarkupLine(await _client.GetMessageAsync(_user.Jwt, encryptionId));
                            break;

                        case "Расшифровать":
                            useSavedSettings = await AnsiConsole.ConfirmAsync("Использовать сохраненные настройки?");

                            int decryptionRowCount;
                            string decryptionKey;
                            if (useSavedSettings)
                            {
                                decryptionRowCount = _settings.RowCount;
                                decryptionKey = _settings.SecretKey;
                                AnsiConsole.MarkupLine("[yellow]Используются сохраненные настройки:[/]");
                                AnsiConsole.MarkupLine($"[blue]Количество строк:[/] {decryptionRowCount}");
                                AnsiConsole.MarkupLine($"[blue]Ключ:[/] {decryptionKey}");
                            }
                            else
                            {
                                decryptionRowCount =
                                    AnsiConsole.Ask<int>("Введите количество строк для расшифрования:");
                                decryptionKey = AnsiConsole.Ask<string>("Введите ключ для расшифрования:");
                            }

                            var decryptionId = AnsiConsole.Ask<int>("Введите номер сообщения:");

                            await _client.DecryptTextAsync(_user.Jwt, decryptionRowCount.ToString(), decryptionKey,
                                decryptionId);
                            AnsiConsole.MarkupLine(await _client.GetMessageAsync(_user.Jwt, decryptionId));
                            break;


                        case "Добавить сообщение":
                            await _client.AddMessageAsync(_user.Jwt,
                                AnsiConsole.Prompt(new TextPrompt<string>("Введите сообщение:")));
                            break;
                        case "Посмотреть сообщения":
                            var messages = await _client.GetMessagesAsync(_user.Jwt);
                            var table = new Table()
                                .BorderColor(Color.Green)
                                .AddColumn("Номер")
                                .AddColumn("Сообщение");
                            foreach (var entry in messages)
                                table.AddRow(entry.MessageNumber.ToString(), entry.Text);
                            AnsiConsole.Write(table);
                            break;
                        case "Получить сообщение":
                            var messageValue = await _client.GetMessageAsync(_user.Jwt,
                                AnsiConsole.Prompt(new TextPrompt<int>("Введите номер сообщения:")));
                            AnsiConsole.MarkupLine($"[green]Сообщение:[/] {messageValue}");
                            break;
                        case "Выход из программы":
                            return;
                        case "Настройки шифрования":

                            AnsiConsole.MarkupLine("[yellow]Текущие настройки:[/]");
                            AnsiConsole.MarkupLine($"[blue]Количество строк:[/] {_settings.RowCount}");
                            AnsiConsole.MarkupLine($"[blue]Ключ:[/] {_settings.SecretKey}");

                            var newRowCount =
                                AnsiConsole.Ask(
                                    "Введите новое количество строк для шифрования (или оставьте текущее значение):",
                                    _settings.RowCount);
                            var newSecretKey = AnsiConsole.Ask<string>(
                                "Введите новый ключ для шифрования (или оставьте текущий):", _settings.SecretKey);

                            var newSettings = new Settings
                            {
                                RowCount = newRowCount,
                                SecretKey = newSecretKey
                            };

                            SettingsManager.SaveSettings(newSettings);
                            AnsiConsole.MarkupLine("[green]Настройки успешно обновлены![/]");
                            break;
                    }

                    break;
            }
        }
    }
}