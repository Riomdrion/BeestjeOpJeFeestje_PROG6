using System.Security.Claims;
using BeestjeOpJeFeestje_PROG6.Controllers;
using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.data.Models;
using BeestjeOpJeFeestje_PROG6.Services;
using BeestjeOpJeFeestje_PROG6.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BeestjeOpJeFeestje_PROG6.unitTest.Controllers
{
    [TestFixture]
    public class BookingControllerTests
    {
        private ApplicationDbContext _dbContext;
        private BookingController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _dbContext.Animals.AddRange(
                new Animal { Name = "Lion", Type = "Predator", Price = 100, ImageUrl = "lion.jpg" },
                new Animal { Name = "Cow", Type = "Farm", Price = 50, ImageUrl = "cow.jpg" }
            );

            var password = "TestPassword123";
            var encryptedPassword = PasswordService.EncryptPassword(password);

            var user = new User
            {
                Email = "test@test.com",
                Role = 0,
                PasswordHash = encryptedPassword,
                Card = "Gold"
            };
            _dbContext.Users.Add(user);

            _dbContext.Bookings.Add(new Booking
            {
                EventDate = DateTime.Today,
                UserId = user.Id,
                Animals = _dbContext.Animals.ToList(),
                Price = 150,
                Discount = 10
            });

            _dbContext.SaveChanges();

            _controller = new BookingController(_dbContext);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("UserId", user.Id.ToString()),
                new Claim("Role", user.Role.ToString()),
                new Claim("Card", user.Card)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Configure TempData
            _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            {
                ["Message"] = "",
                ["AlertClass"] = ""
            };

            _controller.HttpContext.Session = new MockHttpSession();
            _controller.HttpContext.Session.SetString("EventDate", DateTime.Today.ToString("yyyy-MM-dd"));
        }


        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            _controller.Dispose();
        }

        [Test]
        public void StepOne_ReturnsView()
        {
            // Act
            var result = _controller.StepOne() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null, "ViewResult should not be null");
        }

        [Test]
        public async Task StepTwo_ReturnsAvailableAnimals()
        {
            // Arrange
            var eventDate = DateTime.Today.ToString("yyyy-MM-dd");
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockHttpSession();
            httpContext.Session.SetString("EventDate", eventDate);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.StepTwo() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null, "ViewResult should not be null");
            var viewModel = result?.Model as StepTwoVM;
            Assert.That(viewModel, Is.Not.Null, "ViewModel should not be null");
            Assert.That(viewModel?.AvailableAnimals.Count, Is.EqualTo(2), "There should be 2 available animals");
        }

        [Test]
        public async Task SaveAnimals_ReturnsValidationErrors_WhenInvalid()
        {
            // Arrange
            var selectedAnimals = new List<string> { "999" }; // Niet-bestaande dieren
            _controller.HttpContext.Session.SetString("EventDate", DateTime.Today.ToString("yyyy-MM-dd"));

            // Act
            var result = await _controller.SaveAnimals(selectedAnimals) as ViewResult;

            // Assert
            //Assert.That(result, Is.Not.Null, "ViewResult should not be null");
            Assert.That(result?.ViewName, Is.EqualTo("StepTwo"), "Should return StepTwo view");
            Assert.That(result?.ViewData.ModelState.IsValid, Is.False, "ModelState should be invalid");
        }

        [Test]
        public void Delete_RemovesBooking()
        {
            // Arrange
            var booking = _dbContext.Bookings.FirstOrDefault();
            Assert.That(booking, Is.Not.Null, "Booking should exist");

            Console.WriteLine($"Booking found with Id: {booking.Id}");

            // Act
            var result = _controller.Delete(booking.Id) as RedirectToActionResult;

            // Assert
            Console.WriteLine("Delete method executed");
            Assert.That(result, Is.Not.Null, "RedirectToActionResult should not be null");
            Assert.That(result?.ActionName, Is.EqualTo("Read"), "Action name should be 'Read'");
            Assert.That(_dbContext.Bookings.Any(b => b.Id == booking.Id), Is.False,
                "The booking should no longer be in the database");

            Console.WriteLine("Test completed successfully");
        }

    }

    // Mock Http Session
    public class MockHttpSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();

        public string Id { get; } = Guid.NewGuid().ToString();
        public bool IsAvailable { get; } = true;
        public IEnumerable<string> Keys => _sessionStorage.Keys;
        public void Clear() => _sessionStorage.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _sessionStorage.Remove(key);
        public void Set(string key, byte[] value) => _sessionStorage[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);

        public void SetString(string key, string value) =>
            _sessionStorage[key] = System.Text.Encoding.UTF8.GetBytes(value);

        public string GetString(string key) => _sessionStorage.ContainsKey(key)
            ? System.Text.Encoding.UTF8.GetString(_sessionStorage[key])
            : null;
    }

}
