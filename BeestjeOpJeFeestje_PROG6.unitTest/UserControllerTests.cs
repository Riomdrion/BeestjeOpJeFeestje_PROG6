using BeestjeOpJeFeestje_PROG6.Controllers;
using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.data.Models;
using BeestjeOpJeFeestje_PROG6.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace BeestjeOpJeFeestje_PROG6.unitTest
{
    [TestFixture]
    public class UserControllerTests
    {
        private ApplicationDbContext _dbContext;
        private UserController _controller;
        private Mock<HttpContext> _httpContextMock;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _dbContext.Users.Add(new User { Id = 1, Email = "test1@example.com", PasswordHash = "encryptedPassword", Role = 1, Card = "Silver", PhoneNumber = 0612345678 });
            _dbContext.Users.Add(new User { Id = 2, Email = "test2@example.com", PasswordHash = "encryptedPassword", Role = 0, Card = "Gold", PhoneNumber = 0612345679 });
            _dbContext.SaveChanges();

            _httpContextMock = new Mock<HttpContext>();

            // Configureer TempData
            _controller = new UserController(_dbContext);
            _controller.TempData = new TempDataDictionary(_httpContextMock.Object, Mock.Of<ITempDataProvider>())
            {
                ["Message"] = "",
                ["AlertClass"] = ""
            };
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            _controller.Dispose();
        }

        [Test]
        public void Login_ReturnsViewResult_WithLoginViewModel()
        {
            // Act
            var result = _controller.Login();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.InstanceOf<LoginViewModel>());
        }

        [Test]
        public async Task Login_WithInvalidCredentials_ReturnsView()
        {
            // Arrange
            var model = new LoginViewModel { Email = "test@example.com", Password = "WrongPassword" };

            // Act
            var result = await _controller.Login(model);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.EqualTo(model));
        }

        [Test]
        public async Task Logout_RedirectsToLogin()
        {
            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectToActionResult = result as RedirectToActionResult;
            Assert.That(redirectToActionResult?.ActionName, Is.EqualTo("Login"));
        }

        [Test]
        public void Read_ReturnsViewResult_WithUserList()
        {
            // Act
            var result = _controller.Read() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null, "ViewResult should not be null");

            var model = result?.Model as List<UserViewModel>;
            Assert.That(model, Is.Not.Null, "Model should not be null");
            Assert.That(model?.Count, Is.EqualTo(2), "Model should contain exactly 2 users");

            // Debugging: Check the actual users in the model
            foreach (var user in model)
            {
                Console.WriteLine($"User: {user.Email}");
            }
        }

        [Test]
        public void Upsert_Get_NewUser_ReturnsViewWithGeneratedPassword()
        {
            // Act
            var result = _controller.Upsert((int?)null);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as UserViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model?.Password, Is.Not.Empty);
        }

        [Test]
        public void Upsert_Post_CreateNewUser_RedirectsToRead()
        {
            // Arrange
            var model = new UserViewModel
            {
                Email = "newuser@example.com",
                Role = "1", // Admin role
                Card = "5678",
                PhoneNumber = 0612345679,
                Password = "password123"
            };

            // Act
            var result = _controller.Upsert(model);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectToActionResult = result as RedirectToActionResult;
            Assert.That(redirectToActionResult?.ActionName, Is.EqualTo("Read"));

            // Verify that the user was added to the database
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == model.Email);
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Email, Is.EqualTo(model.Email));
        }

        [Test]
        public void Upsert_Post_UpdateExistingUser_RedirectsToRead()
        {
            // Arrange
            var user = _dbContext.Users.First();
            var model = new UserViewModel
            {
                Id = user.Id,
                Email = "updated@example.com",
                Role = "0", // User role
                Card = "9876",
                PhoneNumber = 0612345670,
                Password = "updatedPassword"
            };

            // Act
            var result = _controller.Upsert(model);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectToActionResult = result as RedirectToActionResult;
            Assert.That(redirectToActionResult?.ActionName, Is.EqualTo("Read"));

            // Verify that the user was updated
            var updatedUser = _dbContext.Users.FirstOrDefault(u => u.Id == user.Id);
            Assert.That(updatedUser.Email, Is.EqualTo(model.Email));
            Assert.That(updatedUser.PhoneNumber, Is.EqualTo(model.PhoneNumber));
        }

        [Test]
        public async Task Delete_User_RemovesUserFromDatabase_AndRedirectsToRead()
        {
            // Arrange
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == "test1@example.com");
            Assert.That(user, Is.Not.Null, "User should exist in the database");

            // Act
            var result = await _controller.Delete(user.Id) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null, "RedirectToActionResult should not be null");
            Assert.That(result?.ActionName, Is.EqualTo("Read"), "Action name should be 'Read'");
            Assert.That(_dbContext.Users.Any(u => u.Id == user.Id), Is.False,
                "The animal should no longer be in the database");
        }

        [Test]
        public async Task Delete_User_WithForeignKeyConstraintError_DisplaysErrorMessage()
        {
            // Act
            var result = await _controller.Delete(99) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null, "NotFoundResult should not be null");
            Assert.That(result?.StatusCode, Is.EqualTo(404), "Status code should be 404");
        }

        [Test]
        public async Task Login_UserNotFound_ReturnsViewWithErrorMessage()
        {
            // Arrange
            var model = new LoginViewModel { Email = "nonexistent@example.com", Password = "password123" };

            // Mock HttpContext and TempData
            var httpContextMock = new Mock<HttpContext>();
            var tempDataDictionary = new TempDataDictionary(httpContextMock.Object, Mock.Of<ITempDataProvider>());
            _controller.TempData = tempDataDictionary;

            // Act
            var result = await _controller.Login(model);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.Model, Is.EqualTo(model));

            // Check TempData for error message
            Assert.That(_controller.TempData["Message"], Is.EqualTo("Fout: Gebruiker niet gevonden!"));
        }

        [Test]
        public void HashPassword_CorrectlyHashesPassword()
        {
            // Arrange
            var email = "test@example.com";
            var password = "newPassword123";

            // Mock the database to contain the test user
            var user = new User { Email = email, PasswordHash = "" }; // Empty hash to be replaced
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            // Act
            var result = _controller.HashPassword(email, password); // Call the action

            // Assert
            Assert.That(_controller.ViewBag.HashedPassword, Is.Not.Null);
            Assert.That(_controller.ViewBag.HashedPassword, Is.Not.EqualTo(password));  // Ensure the password is actually hashed.
        }


    }
}
