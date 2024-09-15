using System.ComponentModel.DataAnnotations;

namespace Backend;

public class User
{
    [Required] public string Login { get; set; }
    [Required] public string Password { get; set; }
    [Required] [Compare(nameof(Password))] public string ConfirmPassword { get; set; }
}

public class UserChangePassword
{
    [Required] public string Login { get; set; }
    [Required] public string NewPassword { get; set; }
}