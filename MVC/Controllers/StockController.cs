using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers;

public sealed class StockController : Controller
{
    public IActionResult Availability()
    {
        if (!HttpContext.Session.TryGetValue("c_user_id", out _))
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }

    public IActionResult Alerts()
    {
        if (!HttpContext.Session.TryGetValue("c_user_id", out _))
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }

    public IActionResult ReorderRules()
    {
        if (!HttpContext.Session.TryGetValue("c_user_id", out _))
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }
}
