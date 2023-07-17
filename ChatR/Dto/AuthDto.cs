using System.ComponentModel.DataAnnotations;

namespace ChatR.Dto;

public class SignupDto : LoginDto
{
    [Required] public string Email { get; set; }
}

public class LoginDto
{
    [Required] public string Username { get; set; }

    [Required] public string Password { get; set; }
}