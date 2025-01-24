using System.Runtime.InteropServices.JavaScript;
using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.data.Models;
using BeestjeOpJeFeestje_PROG6.Services;
using BeestjeOpJeFeestje_PROG6.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeestjeOpJeFeestje_PROG6.Controllers;

public class BookingController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public IActionResult StepOne()
    {
        if (User.Identity is { IsAuthenticated: false })
        {
            return RedirectToAction("Login", "User");
        }
        return View();
    }
    
    [HttpPost]
    public IActionResult SaveDate(DateOnly EventDate)
    {
        HttpContext.Session.SetString("EventDate", EventDate.ToString("yyyy-MM-dd"));
        return Redirect("StepTwo");
    }
    
    [HttpGet]
    public async Task<IActionResult> StepTwo()
    {
        var eventDate = HttpContext.Session.GetString("EventDate");
        if (string.IsNullOrEmpty(eventDate))
        {
            return RedirectToAction("StepOne");
        }

        var date = DateTime.Parse(eventDate);
        var availableAnimals = await db.Animals
            .Where(a => a.Bookings.All(b => b.EventDate.Date != date.Date))
            .ToListAsync();
        
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "0");
        var user = await db.Users
            .Include(u => u.Card) // Eager loading van de Card-relatie
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        StepTwoVM stepTwoVM = new()
        {
            AvailableAnimals = availableAnimals,
            CanBook = CalculateNumberOfAnimals.GetMaxAnimals(user.Card),
            CanBookVip = CalculateNumberOfAnimals.GetBookingVipStatus(user.Card)
        };

        return View(stepTwoVM);
    }

    [HttpPost]
    public IActionResult SaveAnimals(List<string> SelectedAnimals)
    {
        HttpContext.Session.SetString("SelectedAnimals", string.Join(",", SelectedAnimals));
        return RedirectToAction("StepThree");
    }
    
    [HttpGet]
    public IActionResult StepThree()
    {
        // Haal de eerder opgeslagen gegevens op uit de sessie
        var eventDateStr = HttpContext.Session.GetString("EventDate");
        var selectedAnimalsStr = HttpContext.Session.GetString("SelectedAnimals");

        if (string.IsNullOrEmpty(eventDateStr) || string.IsNullOrEmpty(selectedAnimalsStr))
        {
            return RedirectToAction("StepOne");
        }

        var eventDate = DateTime.Parse(eventDateStr);
        var selectedAnimalNames = selectedAnimalsStr.Split(',');

        // Maak een Booking-object voor de berekeningen
        var booking = new Booking
        {
            EventDate = eventDate,
            Animals = selectedAnimalNames.Select(name => new Animal { Name = name }).ToList()
        };

        // Bereken korting en prijs
        var discount = PriceAndDiscountCalculator.CalculateDiscount(booking);
        var finalPrice = PriceAndDiscountCalculator.CalculatePrice(booking, discount);

        // Vul het ViewModel
        var viewModel = new BookingVM
        {
            EventDate = eventDate,
            Animals = booking.Animals,
            Price = finalPrice,
            Discount = discount
        };

        return View(viewModel);
    }

    
    [HttpPost]
    public IActionResult Finalize()
    {
        var eventDate = HttpContext.Session.GetString("EventDate");
        var selectedAnimals = HttpContext.Session.GetString("SelectedAnimals")?.Split(',');
        var price = selectedAnimals?.Length * 10 ?? 0;
        var discount = selectedAnimals?.Length > 2 ? 5 : 0;

        var booking = new Booking
        {
            EventDate = DateTime.Parse(eventDate),
            Animals = selectedAnimals?.Select(name => new Animal { Name = name }).ToList(),
            Price = price - discount,
            Discount = discount
        };

        db.Bookings.Add(booking);
        db.SaveChanges();

        return RedirectToAction("Details", new { id = booking.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Bookings()
    {
        // Controleer of de gebruiker is ingelogd
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Account");
        }

        // Haal de ingelogde gebruiker-ID op
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "0");

        // Controleer of de gebruiker een admin is
        var user = await db.Users.FindAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        List<Booking>? bookings = null;

        if (user.Role == 1)
        {
            // Als admin, haal alle boekingen op
            bookings = await db.Bookings
                .Include(b => b.User)
                .Include(b => b.Animals)
                .ToListAsync();
        }
        else if (user.Role == 0)
        {
            // Alleen eigen boekingen ophalen
            bookings = await db.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Animals)
                .ToListAsync();
        }

        // Maak een ViewModel voor de boekingen
        var bookingVMs = bookings.Select(b => new BookingVM
        {
            Id = b.Id,
            EventDate = b.EventDate,
            Animals = b.Animals,
            Price = b.Price,
            Discount = b.Discount,
            User = b.User
        }).ToList();
        return View(bookingVMs);
    }


}
