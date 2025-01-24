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

        var cardValue = User.Claims.FirstOrDefault(c => c.Type == "Card")?.Value;
        var date = DateTime.Parse(eventDate);
        var availableAnimals = await db.Animals
            .Where(a => a.Bookings.All(b => b.EventDate.Date != date.Date) &&
                        (CalculateNumberOfAnimals.GetBookingVipStatus(cardValue) || a.Type != "vip"))
            .OrderBy(a => a.Type)
            .ToListAsync();

        var canBook = CalculateNumberOfAnimals.GetMaxAnimals(cardValue);
        
        var viewModel = new StepTwoVM
        {
            AvailableAnimals = availableAnimals,
            CanBook = canBook
        };
        

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult SaveAnimals(List<string> selectedAnimals)
    {
        var cardValue = User.Claims.FirstOrDefault(c => c.Type == "Card")?.Value;
        var canBook = CalculateNumberOfAnimals.GetMaxAnimals(cardValue);
        
        // Haal de geselecteerde dieren op uit de database of een service
        var selectedAnimalDetails = db.Animals.Where(a => selectedAnimals.Select(int.Parse).Contains(a.Id)).ToList();

        // Valideer de boeking
        var validationErrors = BookingValidationService.ValidateBooking(selectedAnimalDetails, canBook, DateTime.Today);

        // Als er validatiefouten zijn, toon deze en ga terug naar de vorige stap
        if (validationErrors.Any())
        {
            foreach (var error in validationErrors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View("StepTwo");
        }

        // Als alle validaties slagen, sla de selectie op in de sessie
        HttpContext.Session.SetString("SelectedAnimals", string.Join(",", selectedAnimals));

        return RedirectToAction("StepThree");
    }


    [HttpGet]
    public async Task<IActionResult> StepThree()
    {
        var cardValue = User.Claims.FirstOrDefault(c => c.Type == "Card")?.Value ?? "Geen";

        // Haal de eerder opgeslagen gegevens op uit de sessie
        var eventDateStr = HttpContext.Session.GetString("EventDate");
        var selectedAnimalsStr = HttpContext.Session.GetString("SelectedAnimals");

        if (string.IsNullOrEmpty(eventDateStr) || string.IsNullOrEmpty(selectedAnimalsStr))
        {
            return RedirectToAction("StepOne");
        }

        var eventDate = DateTime.Parse(eventDateStr);
        var selectedAnimalIds = selectedAnimalsStr.Split(',').Select(int.Parse).ToList();

        // Maak een Booking-object voor de berekeningen
        var booking = new Booking
        {
            EventDate = eventDate,
            Animals = await db.Animals.Where(a => selectedAnimalIds.Contains(a.Id)).ToListAsync()
        };

        // Bereken korting en prijs
        var discount = PriceAndDiscountCalculator.CalculateDiscount(booking, cardValue);
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
    public async Task<IActionResult> Read()
    {
        // Controleer of de gebruiker is ingelogd
        if (User.Identity is { IsAuthenticated: false })
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