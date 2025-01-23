using BeestjeOpJeFeestje_PROG6.ViewModel;
using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BeestjeOpJeFeestje_PROG6.Services;

namespace BeestjeOpJeFeestje_PROG6.Controllers;

public class UserController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly PasswordHasher<string> _passwordHasher;

    public UserController(ApplicationDbContext db)
    {
        _db = db;
        _passwordHasher = new PasswordHasher<string>();
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View("Login", new UserViewModel());
    }
    
    [HttpGet]
    public IActionResult Read()
    {
        var userViewModels = _db.Users.Select(u => new UserViewModel
        {
            Id = u.Id,
            Email = u.Email,
            Card = u.Card
        }).ToList();

        return View("Read", userViewModels);
    }

    [HttpPost]
    public IActionResult Login(UserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user != null)
            {
                // Vergelijk het ingevoerde wachtwoord met de gehashte versie in de database
                var result = _passwordHasher.VerifyHashedPassword(user.Email, user.PasswordHash, model.PasswordHash);

                if (result == PasswordVerificationResult.Success)
                {
                    TempData["Message"] = "Login succesvol!";
                    return RedirectToAction("Read", "Animal"); // 🔹 Redirect naar Read pagina
                }
            }

            TempData["Message"] = "Fout: Ongeldige inloggegevens!";
        }

        return View(model);
    }

    [HttpPost]
    public IActionResult HashPassword(string email, string plainPassword)
    {
        if (!string.IsNullOrEmpty(plainPassword) && !string.IsNullOrEmpty(email))
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
        
            if (user != null)
            {
                user.PasswordHash = PasswordService.HashPassword(plainPassword);
                _db.SaveChanges(); // Sla de wijzigingen op in de database

                TempData["Message"] = $"Wachtwoord gehasht en opgeslagen voor {email}!";
            }
            else
            {
                TempData["Message"] = "Gebruiker niet gevonden!";
            }
        }
        else
        {
            TempData["Message"] = "Ongeldige invoer!";
        }

        return View("Login", new UserViewModel()); // Terug naar de loginpagina met een melding
    }



}