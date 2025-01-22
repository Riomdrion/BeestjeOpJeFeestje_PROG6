using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.data.Models;
using Microsoft.AspNetCore.Mvc;

namespace BeestjeOpJeFeestje_PROG6.Controllers;

public class AnimalController(ApplicationDbContext db) : Controller
{
   [HttpGet]
   public IActionResult Read()
   {
      var animalViewModels = db.Animals.Select(a => new AnimalViewModel
      {
         Id = a.Id,
         Name = a.Name,
         Type = a.Type,
         Price = a.Price,
         ImageUrl = a.ImageUrl
      }).ToList();

      // Return the view with the list of AnimalViewModel
      return View(animalViewModels);
   }
   
   [HttpGet]
   public IActionResult Upsert(int id)
   {
      var animal = db.Animals.FirstOrDefault(a => a.Id == id);
      if (animal == null)
      {
         return NotFound();
      }

      var animalViewModel = new AnimalViewModel
      {
         Id = animal.Id,
         Name = animal.Name,
         Type = animal.Type,
         Price = animal.Price,
         ImageUrl = animal.ImageUrl
      };
      return View(animalViewModel);
   }
   
   [HttpPost]
   public IActionResult Upsert(AnimalViewModel animalViewModel)
   {
      if (ModelState.IsValid)
      {
         var animal = new Animal
         {
            Name = animalViewModel.Name,
            Type = animalViewModel.Type,
            Price = animalViewModel.Price,
            ImageUrl = animalViewModel.ImageUrl
         };
         
         db.Animals.Add(animal);
         db.SaveChanges();
         
         return RedirectToAction("Read");
      }
      
      return View(animalViewModel);
   }
}