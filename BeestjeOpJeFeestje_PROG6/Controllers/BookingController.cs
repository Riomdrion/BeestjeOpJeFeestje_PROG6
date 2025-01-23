using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.data.Models;
using BeestjeOpJeFeestje_PROG6.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace BeestjeOpJeFeestje_PROG6.Controllers;

public class BookingController : Controller
{
    private readonly ApplicationDbContext _db;

    public BookingController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult BookingWizard()
    {
        var viewModel = new BookingWizardVM
        {
            AvailableAnimals = _db.Animals.ToList(),
            Booking = new BookingVM()
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult BookingWizard(BookingWizardVM viewModel)
    {
        if (ModelState.IsValid)
        {
            var booking = new Booking
            {
                EventDate = viewModel.Booking.Date,
                UserId = viewModel.Booking.User.Id,
                User = new User
                {
                    Id = viewModel.Booking.User.Id,
                    Email = viewModel.Booking.User.Email
                },
                AnimalId = viewModel.Booking.Animal.Id,
                IsConfirmed = viewModel.Booking.IsConfirmed
            };

            _db.Bookings.Add(booking);
            _db.SaveChanges();

            return RedirectToAction("Details", new { id = booking.Id });
        }

        viewModel.AvailableAnimals = _db.Animals.ToList();
        return View(viewModel);
    }
}