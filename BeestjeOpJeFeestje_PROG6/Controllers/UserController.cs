using BeestjeOpJeFeestje_PROG6.ViewModel;
using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using BeestjeOpJeFeestje_PROG6;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

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
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    public async Task<IActionResult> Login(LoginViewModel model)
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
                    // Maak claims aan en voeg de UserId toe
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email), // Gebruik Email als Name claim
                        new Claim("UserId", user.Id.ToString()) // Voeg de UserId toe als custom claim
                    };

                    // Maak een ClaimsIdentity
                    var claimsIdentity = new ClaimsIdentity(claims, "Login");

                    // Maak een ClaimsPrincipal
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Log de gebruiker in
                    await HttpContext.SignInAsync(claimsPrincipal);

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

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
    

    [HttpPost]
    public IActionResult HashPassword(string email, string plainPassword)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == email);

        if (user != null)
        {
            // Versleutel wachtwoord en update in de database
            user.PasswordHash = PasswordService.EncryptPassword(plainPassword);
            _db.SaveChanges();

            ViewBag.HashedPassword = user.PasswordHash;
        }
        else
        {
            ViewBag.HashedPassword = "Gebruiker niet gevonden!";
        }

        return View("Login");
    }


    [HttpGet]
    public IActionResult Read()
    {
        var userViewModels = _db.Users
            .Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                Card = u.Card,
                Role = u.Role == 1 ? "Admin" : "User",  // Convert role to readable format
                PhoneNumber = u.PhoneNumber,
            })
            .ToList();

        // Behoud de melding voor de volgende request
        if (TempData["Message"] != null)
        {
            ViewBag.Message = TempData["Message"];
        }
        
        return View("Read", userViewModels);
    }

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
                PhoneNumber = user.PhoneNumber,
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
                    PhoneNumber = model.PhoneNumber,
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
                user.PhoneNumber = model.PhoneNumber;
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
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
                TempData["Message"] = $"{user.Email} is succesvol verwijderd.";
            }
            catch (DbUpdateException ex)
            {
                // Controleer of de fout te maken heeft met een Foreign Key Constraint
                if (ex.InnerException?.Message.Contains("REFERENCE constraint") == true)
                {
                    TempData["ErrorMessage"] = $"Je kunt {user.Email} niet verwijderen omdat deze gebruiker een boeking heeft.";
                    return RedirectToAction("Upsert", new { id = user.Id });  // Terug naar de Upsert-pagina (Edit User)
                }
                else
                {
                    TempData["ErrorMessage"] = "Er is een fout opgetreden bij het verwijderen van de gebruiker.";
                    return RedirectToAction("Upsert", new { id = user.Id });  // Terug naar de Upsert-pagina (Edit User)
                }
            }

            return RedirectToAction("Read");
        }
    }