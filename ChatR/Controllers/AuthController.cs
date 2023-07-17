using ChatR.Dto;
using ChatR.Service;
using Microsoft.AspNetCore.Mvc;

namespace ChatR.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    public async Task<IActionResult> Signup(SignupDto signupDto)
    {
        if (!ModelState.IsValid) return BadRequest();

        var result = await _authService.Signup(signupDto);

        var data = new ResponseData<string>
        {
            Value = result
        };

        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        if (!ModelState.IsValid) return BadRequest();

        var result = await _authService.Login(loginDto);

        var data = new ResponseData<string>
        {
            Value = result
        };

        return Ok(data);
    }
}