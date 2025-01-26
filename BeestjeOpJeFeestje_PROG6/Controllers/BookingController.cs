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
        if (User.Identity is { IsAuthenticated: false } || !User.Claims.Any())
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
    public async Task<IActionResult> SaveAnimals(List<string> selectedAnimals)
    {
        DateTime eventDate = Convert.ToDateTime(HttpContext.Session.GetString("EventDate"));
        var cardValue = User.Claims.FirstOrDefault(c => c.Type == "Card")?.Value;
        var canBook = CalculateNumberOfAnimals.GetMaxAnimals(cardValue);

        var selectedAnimalDetails = db.Animals.Where(a => selectedAnimals.Select(int.Parse).Contains(a.Id)).ToList();
        var availableAnimals = await db.Animals
            .Where(a => a.Bookings.All(b => b.EventDate.Date != eventDate.Date) &&
                        (CalculateNumberOfAnimals.GetBookingVipStatus(cardValue) || a.Type != "vip"))
            .OrderBy(a => a.Type)
            .ToListAsync();

        var validationErrors = BookingValidationService.ValidateBooking(selectedAnimalDetails, canBook, eventDate);
        
        var existingAnimals = db.Animals.Select(a => a.Id.ToString()).ToList();
        var validSelectedAnimals = selectedAnimals.Where(sa => existingAnimals.Contains(sa)).ToList();

        if (validationErrors.Any() && !validSelectedAnimals.Any())
        {
            foreach (var error in validationErrors)
            {
                TempData["Message"] = error;
                TempData["AlertClass"] = "error";
                ModelState.AddModelError(string.Empty, error);
            }

            var viewmodel = new StepTwoVM
            {
                AvailableAnimals = db.Animals.ToList(),
                CanBook = canBook
            };
            return View("StepTwo", viewmodel);
        }

        HttpContext.Session.SetString("SelectedAnimals", string.Join(",", validSelectedAnimals));
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
        var selectedAnimals = await db.Animals.Where(a => selectedAnimalIds.Contains(a.Id)).ToListAsync();
        double price = selectedAnimals.Sum(a => a.Price);

        // Bereken korting en prijs
        var discount = PriceAndDiscountCalculator.CalculateDiscount(DateOnly.FromDateTime(eventDate), selectedAnimals, cardValue);
        var finalPrice = PriceAndDiscountCalculator.CalculatePrice(price, discount);

        // Vul het ViewModel
        var viewModel = new BookingVM
        {
            EventDate = eventDate,
            Animals = selectedAnimals,
            Price = finalPrice,
            Discount = discount
        };

        return View(viewModel);
    }


    [HttpPost]
    public async Task<IActionResult> Finalize(BookingVM bookingvm)
    {
        var selectedAnimalsStr = HttpContext.Session.GetString("SelectedAnimals");
        var selectedAnimalIds = selectedAnimalsStr.Split(',').Select(int.Parse).ToList();
        var selectedAnimals = await db.Animals.Where(a => selectedAnimalIds.Contains(a.Id)).ToListAsync();
        
        // Maak een nieuwe booking en koppel de dieren
        var value = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (value != null)
        {
            var booking = new Booking
            {
                EventDate = bookingvm.EventDate,
                Animals = selectedAnimals,
                Price = bookingvm.Price,
                Discount = bookingvm.Discount,
                UserId = int.Parse(value)
            };

            db.Bookings.Add(booking);
            db.SaveChanges();

            return RedirectToAction("Read", new { id = booking.Id });
        }
        return RedirectToAction("Login", "User");
    }


    [HttpGet]
    public async Task<IActionResult> Read()
    {
        // Controleer of de gebruiker is ingelogd
        if (User.Identity is { IsAuthenticated: false })
        {
            return RedirectToAction("Login", "User");
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
    
    [HttpPost]
    public IActionResult Delete(int id)
    {
        Console.WriteLine($"Attempting to delete Booking with Id: {id}");

        var booking = db.Bookings
            .Include(b => b.Animals)
            .FirstOrDefault(b => b.Id == id);

        if (booking != null)
        {
            Console.WriteLine($"Booking found with Id: {id}. Deleting...");
            db.Bookings.Remove(booking);
            db.SaveChanges();

            Console.WriteLine($"Booking with Id: {id} successfully deleted.");
            TempData["Message"] = "Booking successfully deleted.";
            TempData["AlertClass"] = "success";
        }
        else
        {
            Console.WriteLine($"Booking with Id: {id} not found.");
            TempData["Message"] = "Booking not found.";
            TempData["AlertClass"] = "error";
        }

        return RedirectToAction("Read");
    }
}