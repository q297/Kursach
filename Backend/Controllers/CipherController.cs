using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Authorize]
[Route("api/cipher")]
[Consumes("application/json")]
[Produces("application/json")]
public class CipherController(ILogger<Controller> logger) : ControllerBase
{
    private readonly ILogger _logger = logger;
    private readonly SqlCipherController _sqlCipherController = CipherResositoryFactory.CreateSqlCipherController();

    [HttpPost]
    public async Task<IActionResult> AddTextAsync([FromBody] string request)
    {
        var login = User.Identity!.Name!;
        await _sqlCipherController.AddTextAsync(request, login);
        return Ok();
    }

    [HttpGet("{id:int?}")]
    public async Task<IActionResult> GetTextAsync([FromRoute] int? id)
    {
        var login = User.Identity!.Name!;
        string? text;
        if (id == null)
            text = await _sqlCipherController.GetTextAsync(id = null, login);
        else
            text = await _sqlCipherController.GetTextAsync(id, login);
        return text == null ? NotFound("Сообщение не найдено") : Ok(text);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> ChangeTextAsync([FromRoute] int id, [FromBody] string request)
    {
        var login = User.Identity!.Name!;
        await _sqlCipherController.ChangeTextAsync(id, request, login);
        return Ok("Текст изменён");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTextAsync([FromRoute] int id)
    {
        var login = User.Identity!.Name!;
        await _sqlCipherController.DeleteTextAsync(id, login);
        return Ok("Текст удалён");
    }

    [HttpPost("encrypt/{id:int}")]
    public async Task<IActionResult> EncryptTextAsync([FromRoute] int id)
    {
        var login = User.Identity!.Name!;
        await _sqlCipherController.EncryptTextAsync(id,login);
        return Ok("Текст зашифрован");
    }

    [HttpPost("decrypt/{id:int}")]
    public async Task<IActionResult> DecryptTextAsync([FromRoute] int id)
    {
        var login = User.Identity!.Name!;
        await _sqlCipherController.DecryptTextAsync(id, login);
        return Ok("Текст расшифрован");
    }
}