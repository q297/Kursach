using Microsoft.AspNetCore.Mvc;
using WebApplication1.Controllers;

namespace Backend;

[ApiController]
[Route("[api]")]
[Produces("application/json")]
public class Controller(UserRepository userRepository) : ControllerBase
{
    private UserRepository _userRepository = userRepository;

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
    [HttpGet]
    public IActionResult GetUserHistory()
    {
        try
        {
            using var userHistory = _userRepository.GetUserHistoryAsync();
            return Ok(new 
            {
                Status = 200,
                Success = true,
                Message = "User history retrieved successfully",
                UserHistory = userHistory
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(new 
            {
                Status = 400,
                Success = false,
                Message = "Error occurred while retrieving user history",
            });
        }
    }
}