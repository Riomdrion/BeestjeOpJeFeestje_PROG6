using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.data.Models;
using BeestjeOpJeFeestje_PROG6.ViewModel;
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
      AnimalViewModel animalViewModel;
      var animal = db.Animals.FirstOrDefault(a => a.Id == id);
      if (animal == null)
      {
         animalViewModel = new AnimalViewModel();
      }
      else
      {

         animalViewModel = new AnimalViewModel
         {
            Id = animal.Id,
            Name = animal.Name,
            Type = animal.Type,
            Price = animal.Price,
            ImageUrl = animal.ImageUrl
         };
      }
      return View(animalViewModel);
   }
   
   [HttpPost]
   public IActionResult Upsert(AnimalViewModel animalViewModel)
   {
        if (string.IsNullOrWhiteSpace(animalViewModel.Name))
        {
            ModelState.AddModelError("Name", "The Name field is required.");
        }

        if (ModelState.IsValid)
      {
         var animal = db.Animals.FirstOrDefault(a => a.Id == animalViewModel.Id);
         if (animal == null)
         {
            animal = new Animal();
            db.Animals.Add(animal);
         }

         animal.Name = animalViewModel.Name;
         animal.Type = animalViewModel.Type;
         animal.Price = animalViewModel.Price;
         animal.ImageUrl = animalViewModel.ImageUrl;

         db.SaveChanges();
         return RedirectToAction("Read");
      }

      return View("Upsert",animalViewModel);
   }
   
   [HttpPost]
   [ValidateAntiForgeryToken]
   public async Task<IActionResult> Delete(int id)
   {
      Console.WriteLine($"Attempting to delete Animal with Id: {id}");
      var animal = await db.Animals.FindAsync(id);
      if (animal == null)
      {
         Console.WriteLine($"Animal with Id: {id} not found");
         return NotFound();
      }

      Console.WriteLine($"Deleting Animal: {animal.Name}");
      db.Animals.Remove(animal);
      await db.SaveChangesAsync();
      return RedirectToAction("Read");
   }
}