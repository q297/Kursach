using Backend;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api")]
[Consumes("application/json")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public class Controller(ILogger<Controller> logger) : ControllerBase
{
    private readonly UserRepository _userRepository = UserRepositoryFactory.CreateUserRepository();
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Регистрация пользователя
    /// </summary>
    /// <returns>Токен авторизации</returns>
    [HttpPost]
    public async Task<IActionResult> CreateUserAsync([FromBody] User user)
    {
        var path = Request.Path + " " + Request.Method;
        var userId = await _userRepository.AddUserAsync(user);
        try
        {
            _logger.LogInformation("{Path}: User {UserId} created successfully", path, user.Login);
            return Ok(new
            {
                Message = $"User created successfully with ID: {userId}",
                Token = "token"
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Path}: Error occurred while creating user {UserId}", path, user.Login);
            return BadRequest(new
            {
                Status = 400,
                Message = "Error occurred while creating user",
            });
        }
    }

    /// <summary>
    /// Получить историю пользователя
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserHistoryAsync(string id)
    {
        var path = Request.Path + " " + Request.Method;
        var userHistory = await _userRepository.GetUserHistoryAsync(id);
        _logger.LogInformation("{Path}: User history retrieved", path);

        IEnumerable<UserHistory> userHistories = userHistory as UserHistory[] ?? userHistory.ToArray();
        return userHistories.Any()
            ? Ok(new
            {
                Status = 200,
                Success = true,
                History = userHistories.ToArray()
            })
            : Ok(new
            {
                Status = 200,
                Success = true,
                Message = "User history is empty",
            });
    }
    /// <summary>
    /// Удалить историю пользователя
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteHistoryUserAsync(string id)
    {
        var path = Request.Path + " " + Request.Method;
        var affectedRows = await _userRepository.DeleteUserHistoryAsync(id);
        _logger.LogInformation("{Path}: User history deleted", path);
        return affectedRows == 0
            ? Ok(new
            {
                Status = 200,
                Success = true,
                Message = "User history is empty",
            })
            : Ok(new
            {
                Status = 200,
                Success = true,
                Message = "Deleted rows: " + affectedRows,
            });
    }
    
    /// <summary>
    /// Изменить пароль
    /// </summary>
    [HttpPatch]
    public async Task<IActionResult> PatchPasswordAsync([FromBody] UserChangePassword request)
    {
        var path = Request.Path + " " + Request.Method;
        try
        {
            await _userRepository.ChangeUserPasswordAsync(request.NewPassword, request.Login);
            _logger.LogInformation("{Path}: User password changed", path);
    
            return Ok(new
            {
                Status = 200,
                Success = true,
                Message = "User password changed",
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Path}: Error occurred while changing user password", path);
            return BadRequest(new
            {
                Status = 400,
                Message = "Error occurred while changing user password",
            });
        }
    }

}