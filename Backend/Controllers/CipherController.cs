using Backend;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Authorize]
[Route("api/cipher")]
[Consumes("application/json")]
[Produces("application/json")]
public class CipherController(SqlCipherControllerFactory factory) : ControllerBase
{
    private readonly SqlCipherController _sqlCipherController = factory.CreateSqlCipherController();

    /// <summary>
    ///     Добавить текст
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddTextAsync([FromBody] string request)
    {
        var login = User.Identity!.Name!;
        await _sqlCipherController.AddTextAsync(request, login);
        return Ok();
    }

    /// <summary>
    ///     Получить одно сообщение
    /// </summary>
    /// <param name="id">Номер сообщения</param>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTextAsync([FromRoute] int id)
    {
        var login = User.Identity!.Name!;
        var text = (await _sqlCipherController.GetTextAsync(id, login));
        return text == null ? NotFound("Сообщение не найдено") : Ok(text);
    }

    /// <summary>
    ///     Получить все тексты
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTextAsync()
    {
        var login = User.Identity!.Name!;
        var text = await _sqlCipherController.GetTextAsync(login);
        return text == null ? NotFound("Сообщения отсутствуют") : Ok(text);
    }

    /// <summary>
    ///     Изменить текст
    /// </summary>
    /// <param name="id">Номер текста</param>
    /// <param name="request">Строка</param>
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> ChangeTextAsync([FromRoute] int id, [FromBody] string request)
    {
        var login = User.Identity!.Name!;
        await _sqlCipherController.ChangeTextAsync(id, request, login);
        return Ok("Текст изменён");
    }

    /// <summary>
    ///     Удалить текст
    /// </summary>
    /// <param name="id">Номер текста</param>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTextAsync([FromRoute] int id)
    {
        var login = User.Identity!.Name!;
        await _sqlCipherController.DeleteTextAsync(id, login);
        return Ok("Текст удалён");
    }

    /// <summary>
    ///     Зашифровать текст
    /// </summary>
    /// <param name="id">Номер текста</param>
    /// <param name="cipherUserSettings">Параметры для шифра табличной перестановки</param>
    [HttpPost("encrypt/{id:int}")]
    public async Task<IActionResult> EncryptTextAsync([FromRoute] int id,
        [FromBody] CipherUserSettings cipherUserSettings)
    {
        try
        {
            var login = User.Identity!.Name!;
            await _sqlCipherController.EncryptTextAsync(id, login, cipherUserSettings);
            return Ok("Текст зашифрован");
        }
        catch (ArgumentException)
        {
            return NotFound(new { Message = id });
        }
    }

    /// <summary>
    ///     Расшифровать текст
    /// </summary>
    /// <param name="id">Номер текста</param>
    /// <param name="cipherUserSettings">Параметры для шифра табличной перестановки</param>
    [HttpPost("decrypt/{id:int}")]
    public async Task<IActionResult> DecryptTextAsync([FromRoute] int id,
        [FromBody] CipherUserSettings cipherUserSettings)
    {
        try
        {
            var login = User.Identity!.Name!;
            await _sqlCipherController.DecryptTextAsync(id, login, cipherUserSettings);
            return Ok("Текст расшифрован");
        }
        catch (ArgumentException)
        {
            return NotFound(new { Message = id });
        }
    }
}