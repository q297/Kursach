using Microsoft.AspNetCore.Mvc;

namespace Backend;

[ApiController]
[Route("[api]")]
public class Controller : ControllerBase
{
    [HttpPost]
    public IActionResult CreateUser([FromBody] User user)
    {
        
    }
    
}