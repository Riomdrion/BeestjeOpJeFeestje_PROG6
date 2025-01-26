﻿using System.Security.Claims;
using BeestjeOpJeFeestje_PROG6.Controllers;
using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.data.Models;
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
                new Animal { Id = 1, Name = "Lion", Type = "Predator", Price = 100, ImageUrl = "lion.jpg" },
                new Animal { Id = 2, Name = "Cow", Type = "Farm", Price = 50, ImageUrl = "cow.jpg" }
            );

            var user = new User
            {
                Id = 1,
                Email = "test@test.com",
                Role = 0,
                PasswordHash = "hashedPassword",
                Card = "Gold"
            };
            _dbContext.Users.Add(user);

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

            _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            _controller.HttpContext.Session = new MockHttpSession();
            _controller.HttpContext.Session.SetString("EventDate", System.DateTime.Today.ToString("yyyy-MM-dd"));
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
            var result = _controller.StepOne() as ViewResult;

            Assert.That(result, Is.Not.Null, "ViewResult should not be null");
        }

        [Test]
        public async Task StepTwo_ReturnsAvailableAnimals()
        {
            var result = await _controller.StepTwo() as ViewResult;

            Assert.That(result, Is.Not.Null, "ViewResult should not be null");

            var viewModel = result?.Model as StepTwoVM;
            Assert.That(viewModel, Is.Not.Null, "ViewModel should not be null");
            Assert.That(viewModel?.AvailableAnimals.Count, Is.EqualTo(2), "There should be 2 available animals");
        }

        [Test]
        public async Task SaveAnimals_ReturnsValidationErrors_WhenInvalid()
        {
            var selectedAnimals = new List<string> { "999" }; // Invalid animal ID

            var result = await _controller.SaveAnimals(selectedAnimals) as ViewResult;

            Assert.That(result, Is.Not.Null, "ViewResult should not be null");
            Assert.That(result?.ViewName, Is.EqualTo("StepTwo"), "Should return StepTwo view");
            Assert.That(result?.ViewData.ModelState.IsValid, Is.False, "ModelState should be invalid");
        }

        [Test]
        public void Delete_RemovesBooking()
        {
            var booking = new Booking
            {
                Id = 1,
                UserId = 1,
                AnimalId = 1,
                EventDate = DateTime.Today
            };

            _dbContext.Bookings.Add(booking);
            _dbContext.SaveChanges();

            var result = _controller.Delete(booking.Id) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null, "RedirectToActionResult should not be null");
            Assert.That(result?.ActionName, Is.EqualTo("Read"), "Action name should be 'Read'");
            Assert.That(_dbContext.Bookings.Any(b => b.Id == booking.Id), Is.False, "The booking should no longer exist in the database");
        }

        private class MockHttpSession : ISession
        {
            private readonly Dictionary<string, byte[]> _sessionStorage = new();

            public string Id { get; } = Guid.NewGuid().ToString();
            public bool IsAvailable { get; } = true;
            public IEnumerable<string> Keys => _sessionStorage.Keys;

            public void Clear() => _sessionStorage.Clear();
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void Remove(string key) => _sessionStorage.Remove(key);
            public void Set(string key, byte[] value) => _sessionStorage[key] = value;
            public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);

            public void SetString(string key, string value) => _sessionStorage[key] = System.Text.Encoding.UTF8.GetBytes(value);

            public string GetString(string key) => _sessionStorage.ContainsKey(key)
                ? System.Text.Encoding.UTF8.GetString(_sessionStorage[key])
                : null;
        }
    }
}
