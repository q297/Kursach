using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api")]
[Consumes("application/json")]
[Produces("application/json")]
public class Controller(ILogger<Controller> logger, UserRepositoryFactory userRepositoryFactory) : ControllerBase
{
    private readonly ILogger _logger = logger;
    private readonly UserRepository _userRepository = userRepositoryFactory.CreateUserRepository();

    /// <summary>
    ///     Регистрация пользователя и возврат JWT токена
    /// </summary>
    /// <returns>Токен авторизации</returns>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUserAsync([FromBody] User user)
    {
        if (_userRepository.GetUserAsync(user.Login).Result != 0) return BadRequest("User already exist");
        var path = Request.Path + " " + Request.Method;

        var userId = await _userRepository.AddUserAsync(user);
        _logger.LogInformation("{Path}: User {UserId} created successfully", path, user.Login);
        await _userRepository.AddUserHistoryAsync(new UserHistory(user.Login,
            "Регистрация", ""));
        return Ok(new
        {
            Message = $"User created successfully with ID: {userId}",
            UserId = userId,
            Token = CreateJwt(user.Login),
            Expiry = DateTime.UtcNow.Add(TimeSpan.FromDays(1))
        });
    }

    /// <summary>
    ///     Авторизация
    /// </summary>
    /// <param name="user"></param>
    /// <returns>Возвращается JWT токен </returns>
    [HttpPost("login")]
    public async Task<IActionResult> LoginUserAsync([FromBody] User user)
    {
        var path = Request.Path + " " + Request.Method;
        var userId = await _userRepository.CheckUserAsync(user);
        if (userId == 0) return Unauthorized(new { Message = "Incorrect login or password" });

        await _userRepository.AddUserHistoryAsync(new UserHistory(user.Login,
            "Вход", "Возвращен токен"));
        _logger.LogInformation("{Path}: User successfully enter. Return the token", path);
        return Ok(new
        {
            UserId = userId,
            Token = CreateJwt(user.Login),
            Expiry = DateTime.UtcNow.Add(TimeSpan.FromDays(1))
        });
    }

    /// <summary>
    ///     Получить историю пользователя
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUserHistoryAsync()
    {
        var login = User.Identity?.Name;
        var path = Request.Path + " " + Request.Method;
        var userHistory = await _userRepository.GetUserHistoryAsync(login);
        _logger.LogInformation("{Path}: User history retrieved", path);

        IEnumerable<UserHistory> userHistories = userHistory as UserHistory[] ?? userHistory.ToArray();
        return userHistories.Any()
            ? Ok(new
            {
                userHistories
            })
            : Ok(new
            {
                Message = "User history is empty"
            });
    }

    /// <summary>
    ///     Удалить историю пользователя
    /// </summary>
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteHistoryUserAsync()
    {
        var login = User.Identity?.Name;
        var path = Request.Path + " " + Request.Method;
        var affectedRows = await _userRepository.DeleteUserHistoryAsync(login);
        _logger.LogInformation("{Path}: User history deleted", path);
        return affectedRows == 0
            ? Ok(new
            {
                Success = true,
                Message = "User history is empty"
            })
            : Ok(new
            {
                Success = true,
                Message = "Deleted rows: " + affectedRows
            });
    }

    /// <summary>
    ///     Изменить пароль
    /// </summary>
    [HttpPatch]
    public async Task<IActionResult> PatchPasswordAsync([FromBody] UserChangesPassword request)
    {
        var path = Request.Path + " " + Request.Method;
        if (!await _userRepository.ChangeUserPasswordAsync(request))
        {
            _logger.LogInformation("{Path}: Неправильный логин или пароль", path);
            return Unauthorized(new
            {
                Message = "Incorrect login or password"
            });
        }

        _logger.LogInformation("{Path}: User password changed", path);
        await _userRepository.AddUserHistoryAsync(new UserHistory(request.Login,
            "Смена пароля", $"Старый пароль {new string('*', request.Password.Length)}"));

        return Ok(new
        {
            Message = "Password changed successfully",
            Token = CreateJwt(request.Login)
        });
    }

    [NonAction]
    private string CreateJwt(string value)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, value) };
        var jwt = new JwtSecurityToken(
            AuthOptions.Issuer,
            AuthOptions.Audience,
            claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        return encodedJwt;
    }

    [NonAction]
    private string? ReadJWT(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(token)) return null; // токен не найден

        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var login = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
        return login;
    }
}