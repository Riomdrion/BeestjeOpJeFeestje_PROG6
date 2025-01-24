using System.Runtime.InteropServices.JavaScript;
using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.data.Models;
using BeestjeOpJeFeestje_PROG6.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeestjeOpJeFeestje_PROG6.Controllers;

public class BookingController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public IActionResult StepOne()
    {
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
            .Select(a => a.Name)
            .ToListAsync();

        return View(availableAnimals);
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
        var selectedAnimals = HttpContext.Session.GetString("SelectedAnimals")?.Split(',');
        return View(selectedAnimals);
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

}
