using System.ComponentModel.DataAnnotations;

namespace Backend;

public class User
{
    [Required] string Login { get; set; }
    [Required] string Password { get; set; }
    [Required] [Compare(nameof(Password))] private string ConfirmPassword { get; set; }
}