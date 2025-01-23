// using BeestjeOpJeFeestje_PROG6.data.DBcontext;
// using BeestjeOpJeFeestje_PROG6.data.Models;
// using BeestjeOpJeFeestje_PROG6.ViewModel;
// using Microsoft.AspNetCore.Mvc;
//
// namespace BeestjeOpJeFeestje_PROG6.Controllers;
//
// public class BookingController(ApplicationDbContext db) : Controller
//     {
//
//         // Step 1: Select Date
//         [HttpGet]
//         public IActionResult SelectDate()
//         {
//             return View();
//         }
//
//         [HttpPost]
//         public IActionResult SelectDate(DateTime date)
//         {
//             var availableAnimals = db.Animals
//                 .Where(a => !db.Bookings.Any(b => b.AnimalId == a.Id && b.EventDate == date))
//                 .ToList();
//
//             TempData["SelectedDate"] = date;
//             return View("SelectAnimals", availableAnimals);
//         }
//
//         // Step 2: Select Animals
//         [HttpPost]
//         public IActionResult SelectAnimals(List<int> animalIds)
//         {
//             var selectedDate = (DateTime)TempData["SelectedDate"];
//             TempData["AnimalIds"] = animalIds;
//
//             var animals = db.Animals.Where(a => animalIds.Contains(a.Id)).ToList();
//             ViewBag.SelectedAnimals = animals;
//
//             return View("EnterContactInfo");
//         }
//
//         // Step 3: Enter Contact Info
//         [HttpPost]
//         public IActionResult EnterContactInfo(string email, int id)
//         {
//             var selectedDate = (DateTime)TempData["SelectedDate"];
//             var animalId = TempData["AnimalId"];
//
//             var newBooking = new Booking
//             {
//                 EventDate = selectedDate,
//                 User = new User { Id = id, Email = email},
//                 Animal = db.Animals.FirstOrDefault(a => a.Id == (int)animalId),
//                 IsConfirmed = false
//             };
//
//             db.Bookings.Add(newBooking);
//             db.SaveChanges();
//
//             return RedirectToAction("Confirm", new { id = newBooking.Id });
//         }
//
//         // Step 4: Confirm Booking
//         [HttpGet]
//         public IActionResult Confirm(int id)
//         {
//             var booking = db.Bookings
//                 .Where(b => b.Id == id)
//                 .Select(b => new BookingVM
//                 {
//                     Id = b.Id,
//                     Date = b.EventDate,
//                     Animal = b.Animal.FirstOrDefault(),
//                     User = b.User
//                 })
//                 .FirstOrDefault();
//
//             return View(booking);
//         }
//
//         [HttpPost]
//         public IActionResult ConfirmBooking(int id)
//         {
//             var booking = db.Bookings.FirstOrDefault(b => b.Id == id);
//             if (booking != null)
//             {
//                 booking.IsConfirmed = true;
//                 db.SaveChanges();
//             }
//
//             return RedirectToAction("Details", new { id });
//         }
//
//         // Step 5: Booking Details
//         public IActionResult Details(int id)
//         {
//             var booking = db.Bookings
//                 .Where(b => b.Id == id)
//                 .Select(b => new BookingVM
//                 {
//                     Id = b.Id,
//                     Date = b.Date,
//                     Animal = b.Animals.FirstOrDefault(),
//                     User = b.User
//                 })
//                 .FirstOrDefault();
//
//             return View(booking);
//         }
//
//         // Delete Booking
//         public IActionResult Delete(int id)
//         {
//             var booking = db.Bookings.FirstOrDefault(b => b.Id == id);
//             if (booking != null)
//             {
//                 db.Bookings.Remove(booking);
//                 db.SaveChanges();
//             }
//
//             return RedirectToAction("Index");
//         }
//     }
// }
