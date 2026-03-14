using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;
using MVC.Models;

namespace MVC.Controllers;

[ApiController]
[Route("api/account")]
public sealed class AccountApiController : ControllerBase
{
    private readonly IUserInterface _userRepository;

    public AccountApiController(IUserInterface userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] AuthLoginModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Please fill in all required fields." });
        }

        var user = await _userRepository.GetUserByEmailAsync(model.Email, cancellationToken);
        if (user is null || !user.IsActive || !string.Equals(user.PasswordHash, model.Password, StringComparison.Ordinal))
        {
            return Unauthorized(new { success = false, message = "Invalid email or password." });
        }

        HttpContext.Session.SetString("c_user_id", user.UserId.ToString());
        HttpContext.Session.SetString("c_full_name", user.FullName);
        HttpContext.Session.SetString("c_email", user.Email);

        return Ok(new { success = true, message = "Login successful." });
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromForm] AuthSignupModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Please fill in all required fields." });
        }

        var existing = await _userRepository.GetUserByEmailAsync(model.Email, cancellationToken);
        if (existing is not null)
        {
            return Conflict(new { success = false, message = "An account with this email already exists." });
        }

        var user = new UserRecord
        {
            FullName = model.FullName,
            Email = model.Email,
            Phone = model.Phone,
            PasswordHash = model.Password,
            IsActive = true,
        };

        await _userRepository.CreateUserAsync(user, cancellationToken);
        return Ok(new { success = true, message = "Account created. Please log in." });
    }
}
