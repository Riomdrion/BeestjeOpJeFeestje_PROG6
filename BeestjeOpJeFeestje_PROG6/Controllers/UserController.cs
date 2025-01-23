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
        return View("Login", new LoginViewModel());
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user != null)
            {
                // Vergelijk het ingevoerde wachtwoord met de gehashte versie in de database
                var result = _passwordHasher.VerifyHashedPassword(user.Email, user.PasswordHash, model.Password);

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

    [HttpGet]
    public IActionResult Read()
    {
        var userViewModels = _db.Users.Select(u => new UserViewModel
        {
            Id = u.Id,
            Email = u.Email,
            Card = u.Card,
            Role = u.Role == 1 ? "Admin" : "User"  // Zet de rol om naar een leesbare vorm
        }).ToList();

        return View("Read", userViewModels);
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

        return View("Login", new LoginViewModel()); // Terug naar de loginpagina met een melding
    }

    [HttpGet]
        public IActionResult Upsert(int? id)
        {
            UserViewModel userViewModel;

            if (id == null || id == 0)
            {
                // Nieuwe gebruiker aanmaken met een gegenereerd wachtwoord
                userViewModel = new UserViewModel
                {
                    PasswordHash = GenerateRandomPassword(),
                    Role = "0" // Standaardrol: Gebruiker
                };
            }
            else
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                {
                    return NotFound();
                }

                userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    Card = user.Card
                };
            }

            return View(userViewModel);
        }

        [HttpPost]
        public IActionResult Upsert(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == model.Id);

                if (user == null)
                {
                    // Nieuwe user toevoegen met gehasht wachtwoord
                    user = new User
                    {
                        Email = model.Email,
                        Role = int.Parse(model.Role), // Zet string om naar int
                        Card = model.Card,
                        PasswordHash = PasswordService.HashPassword(model.PasswordHash) // Wachtwoord hashen
                    };
                    _db.Users.Add(user);
                    TempData["Message"] = $"User {model.Email} created successfully! Password: {model.PasswordHash}";
                }
                else
                {
                    // Bestaande user updaten
                    user.Email = model.Email;
                    user.Role = int.Parse(model.Role);
                    user.Card = model.Card;
                    TempData["Message"] = $"User {model.Email} updated successfully!";
                }

                _db.SaveChanges();
                return RedirectToAction("Read");
            }

            return View(model);
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }