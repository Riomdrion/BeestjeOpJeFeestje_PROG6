using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace BeestjeOpJeFeestje_PROG6.Controllers;

public class BookingController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public IActionResult Read()
    {
        var bookingViewModels = db.Bookings.Select(b => new BookingVM
        {
            Id = b.UserId,
            Date = b.EventDate,
            AnimalId = b.AnimalId,
            Animal = b.Animal,
            UserId = b.UserId,
            User = b.User
        }).ToList();

        // Return the view with the list of BookingViewModel
        return View(bookingViewModels);
    }
}