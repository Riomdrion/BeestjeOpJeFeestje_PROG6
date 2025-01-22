using BeestjeOpJeFeestje_PROG6.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace BeestjeOpJeFeestje_PROG6.Controllers;

public class UserController : Controller
{
    
    public IActionResult Index()
        {
            return View("Login", new UserViewModel());
        }
}