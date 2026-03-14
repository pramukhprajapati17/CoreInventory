using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using Repositories.Interfaces;
using Repositories.Models;

namespace MVC.Controllers;

public sealed class AccountController : Controller
{
    private readonly IUserInterface _userRepository;

    public AccountController(IUserInterface userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new AuthLoginModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(AuthLoginModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Error = "Please fill in all required fields.";
            return View(model);
        }

        var user = await _userRepository.GetUserByEmailAsync(model.Email, cancellationToken);
        if (user is null || !user.IsActive)
        {
            ViewBag.Error = "Invalid email or password.";
            return View(model);
        }

        if (!string.Equals(user.PasswordHash, model.Password, StringComparison.Ordinal))
        {
            ViewBag.Error = "Invalid email or password.";
            return View(model);
        }

        HttpContext.Session.SetString("c_user_id", user.UserId.ToString());
        HttpContext.Session.SetString("c_full_name", user.FullName);
        HttpContext.Session.SetString("c_email", user.Email);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Signup()
    {
        return View(new AuthSignupModel());
    }

    [HttpPost]
    public async Task<IActionResult> Signup(AuthSignupModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Error = "Please fill in all required fields.";
            return View(model);
        }

        var existing = await _userRepository.GetUserByEmailAsync(model.Email, cancellationToken);
        if (existing is not null)
        {
            ViewBag.Error = "An account with this email already exists.";
            return View(model);
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
        TempData["Success"] = "Account created. Please log in.";
        return RedirectToAction("Login");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
