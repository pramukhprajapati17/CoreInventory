using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers;

public sealed class ProfileController : Controller
{
    public IActionResult Index()
    {
        if (!HttpContext.Session.TryGetValue("c_user_id", out _))
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }
}
