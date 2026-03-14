using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using Repositories.Interfaces;

namespace MVC.Controllers;

[ApiController]
[Route("api/profile")]
public sealed class ProfileApiController : ControllerBase
{
    private readonly IUserInterface _users;

    public ProfileApiController(IUserInterface users)
    {
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!HttpContext.Session.TryGetValue("c_user_id", out var userIdBytes))
        {
            return Unauthorized(new { success = false, message = "Not logged in." });
        }

        var userId = long.Parse(System.Text.Encoding.UTF8.GetString(userIdBytes));
        var user = await _users.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound(new { success = false, message = "User not found." });
        }

        return Ok(new
        {
            fullName = user.FullName,
            email = user.Email,
            phone = user.Phone
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ProfileUpdateModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Please fill required fields." });
        }

        if (!HttpContext.Session.TryGetValue("c_user_id", out var userIdBytes))
        {
            return Unauthorized(new { success = false, message = "Not logged in." });
        }

        var userId = long.Parse(System.Text.Encoding.UTF8.GetString(userIdBytes));
        var updated = await _users.UpdateProfileAsync(userId, model.FullName, model.Phone, model.Password, cancellationToken);
        if (!updated)
        {
            return NotFound(new { success = false, message = "User not found." });
        }

        HttpContext.Session.SetString("c_full_name", model.FullName);
        return Ok(new { success = true, message = "Profile updated." });
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Please fill required fields." });
        }

        if (!string.Equals(model.NewPassword, model.ConfirmPassword, StringComparison.Ordinal))
        {
            return BadRequest(new { success = false, message = "Passwords do not match." });
        }

        if (!HttpContext.Session.TryGetValue("c_user_id", out var userIdBytes))
        {
            return Unauthorized(new { success = false, message = "Not logged in." });
        }

        var userId = long.Parse(System.Text.Encoding.UTF8.GetString(userIdBytes));
        var updated = await _users.UpdatePasswordAsync(userId, model.CurrentPassword, model.NewPassword, cancellationToken);
        if (!updated)
        {
            return BadRequest(new { success = false, message = "Current password is incorrect." });
        }

        return Ok(new { success = true, message = "Password changed." });
    }
}
