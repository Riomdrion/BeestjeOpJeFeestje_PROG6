using BeestjeOpJeFeestje_PROG6.ViewModel;
using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BeestjeOpJeFeestje_PROG6;

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
                // Encrypt the entered password to match stored encrypted value
                string encryptedInputPassword = PasswordService.EncryptPassword(model.Password);

                // Debugging: Check what is being compared
                Console.WriteLine($"Stored Encrypted Password: {user.PasswordHash}");
                Console.WriteLine($"Encrypted Entered Password: {encryptedInputPassword}");

                // Compare encrypted input with stored encrypted password
                if (encryptedInputPassword == user.PasswordHash)
                {
                    TempData["Message"] = "Login succesvol!";
                    return RedirectToAction("Read", "Animal"); // Redirect after successful login
                }
                else
                {
                    TempData["Message"] = "Fout: Wachtwoord klopt niet!";
                }
            }
            else
            {
                TempData["Message"] = "Fout: Gebruiker niet gevonden!";
            }
        }

        return View(model);
    }


    [HttpGet]
    public IActionResult Read()
    {
        var userViewModels = _db.Users
            .Where(u => u.Id > 13)  // Filter out users with Id <= 13
            .Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                Card = u.Card,
                Role = u.Role == 1 ? "Admin" : "User"  // Convert role to readable format
            })
            .ToList();

        return View("Read", userViewModels);
    }

    
    // [HttpPost]
    // public IActionResult HashPassword(string email, string plainPassword)
    // {
    //     if (!string.IsNullOrEmpty(plainPassword) && !string.IsNullOrEmpty(email))
    //     {
    //         var user = _db.Users.FirstOrDefault(u => u.Email == email);
    //     
    //         if (user != null)
    //         {
    //             user.PasswordHash = PasswordService.HashPassword(plainPassword);
    //             _db.SaveChanges(); // Sla de wijzigingen op in de database
    //
    //             TempData["Message"] = $"Wachtwoord gehasht en opgeslagen voor {email}!";
    //         }
    //         else
    //         {
    //             TempData["Message"] = "Gebruiker niet gevonden!";
    //         }
    //     }
    //     else
    //     {
    //         TempData["Message"] = "Ongeldige invoer!";
    //     }
    //
    //     return View("Login", new LoginViewModel()); // Terug naar de loginpagina met een melding
    // }

    [HttpGet]
    public IActionResult Upsert(int? id)
    {
        UserViewModel userViewModel;

        if (id == null || id == 0)
        {
            // Nieuwe gebruiker met een gegenereerd wachtwoord
            userViewModel = new UserViewModel
            {
                Password = GenerateRandomPassword(),
                Role = "0"
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
                Card = user.Card,
                Password = PasswordService.DecryptPassword(user.PasswordHash) // Wachtwoord decoderen
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
                user = new User
                {
                    Email = model.Email,
                    Role = int.Parse(model.Role),
                    Card = model.Card,
                    PasswordHash = PasswordService.EncryptPassword(model.Password) // Encrypt before saving
                };
                _db.Users.Add(user);
            }
            else
            {
                // Update existing user
                user.Email = model.Email;
                user.Role = int.Parse(model.Role);
                user.Card = model.Card;
                user.PasswordHash = PasswordService.EncryptPassword(model.Password);
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