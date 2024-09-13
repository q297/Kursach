using Backend;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api")]
[Consumes("application/json")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public class Controller : ControllerBase
{
    private readonly UserRepository _userRepository = UserRepositoryFactory.CreateUserRepository();
    /// <summary>
    /// Регистрация пользователя
    /// </summary>
    /// <returns>Токен авторизации</returns>
    [HttpPost]
    public IActionResult CreateUser([FromBody] User user)
    {
        try
        {
            _userRepository.AddUserAsync(user);
            return Ok(new
            {
                Status = 200,
                Success = true,
                Message = "User created successfully",
                Token = "token"
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(new
            {
                Status = 400,
                Success = false,
                Message = "Error occurred while creating user",
            });
        }
    }

    /// <summary>
    ///  Получить историю пользователей
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserHistoryAsync()
    {
        var userHistory = await _userRepository.GetUserHistoryAsync();

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
}
