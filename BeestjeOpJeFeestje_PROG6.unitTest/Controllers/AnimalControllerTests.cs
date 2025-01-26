using BeestjeOpJeFeestje_PROG6.Controllers;
using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.data.Models;
using BeestjeOpJeFeestje_PROG6.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeestjeOpJeFeestje_PROG6.unitTest.Controllers
{
    [TestFixture]
    public class AnimalControllerTests
    {
        private ApplicationDbContext _dbContext;
        private AnimalController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            _dbContext = new ApplicationDbContext(options);

            // Voeg testdata toe
            _dbContext.Animals.AddRange(
                new Animal { Name = "Lion", Type = "Predator", Price = 100, ImageUrl = "lion.jpg" },
                new Animal { Name = "Cow", Type = "Farm", Price = 50, ImageUrl = "cow.jpg" }
            );
            _dbContext.SaveChanges();

            _controller = new AnimalController(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            _controller.Dispose();
        }

        [Test]
        public void Read_ReturnsAnimalList()
        {
            // Act
            var result = _controller.Read() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null, "ViewResult should not be null");

            var model = result?.Model as List<AnimalViewModel>;
            Assert.That(model, Is.Not.Null, "Model should not be null");
            Assert.That(model?.Count, Is.EqualTo(2), "Model should contain exactly 2 animals");
        }

        [Test]
        public void Upsert_SaveNewAnimal()
        {
            // Arrange
            var newAnimal = new AnimalViewModel
            {
                Name = "Penguin",
                Type = "Snow",
                Price = 80,
                ImageUrl = "penguin.jpg"
            };

            // Act
            var result = _controller.Upsert(newAnimal) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null, "RedirectToActionResult should not be null");
            Assert.That(result?.ActionName, Is.EqualTo("Read"), "Action name should be 'Read'");
            Assert.That(_dbContext.Animals.Count(), Is.EqualTo(3), "There should now be 3 animals in the database");
        }

        [Test]
        public async Task Delete_RemovesAnimal()
        {
            // Arrange
            var animal = _dbContext.Animals.FirstOrDefault(a => a.Name == "Lion");
            Assert.That(animal, Is.Not.Null, "Animal should exist in the database");

            // Act
            var result = await _controller.Delete(animal.Id) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null, "RedirectToActionResult should not be null");
            Assert.That(result?.ActionName, Is.EqualTo("Read"), "Action name should be 'Read'");
            Assert.That(_dbContext.Animals.Any(a => a.Id == animal.Id), Is.False,
                "The animal should no longer be in the database");
        }




        [Test]
        public async Task Delete_ReturnsNotFoundForNonExistentAnimal()
        {
            // Act
            var result = await _controller.Delete(99) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null, "NotFoundResult should not be null");
            Assert.That(result?.StatusCode, Is.EqualTo(404), "Status code should be 404");
        }
    }
}
