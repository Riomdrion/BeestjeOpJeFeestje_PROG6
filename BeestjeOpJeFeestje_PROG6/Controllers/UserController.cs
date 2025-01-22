using BeestjeOpJeFeestje_PROG6.ViewModel;
using Microsoft.AspNetCore.Mvc;

using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using Microsoft.AspNetCore.Mvc;

namespace BeestjeOpJeFeestje_PROG6.Controllers;

public class UserController(ApplicationDbContext db) : Controller
{
    
    public IActionResult Index()
        {
            return View("Login", new UserViewModel());
        }
}