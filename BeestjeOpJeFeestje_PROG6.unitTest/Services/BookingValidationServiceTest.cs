using BeestjeOpJeFeestje_PROG6.Services;
using BeestjeOpJeFeestje_PROG6.data.Models;

namespace BeestjeOpJeFeestje_PROG6.Tests
{
    [TestFixture]
    public class BookingValidationServiceTests
    {
        [Test]
        public void ValidateBooking_ExceedsMaxAnimals_ReturnsError()
        {
            // Arrange
            var selectedAnimals = new List<Animal>
            {
                new Animal { Name = "Lion", Type = "Leeuw" },
                new Animal { Name = "Penguin", Type = "Pinguïn" }
            };
            int maxAnimals = 1;
            var eventDate = DateTime.Now;

            // Act
            var errors = BookingValidationService.ValidateBooking(selectedAnimals, maxAnimals, eventDate);

            // Assert
            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors[0], Is.EqualTo("You can only book up to 1 animals."));
        }

        [Test]
        public void ValidateBooking_FarmAnimalAndPredator_ReturnsError()
        {
            // Arrange
            var selectedAnimals = new List<Animal>
            {
                new Animal { Name = "Farm Animal", Type = "Boerderijdier" },
                new Animal { Name = "Lion", Type = "Leeuw" }
            };
            int maxAnimals = 5;
            var eventDate = DateTime.Now;

            // Act
            var errors = BookingValidationService.ValidateBooking(selectedAnimals, maxAnimals, eventDate);

            // Assert
            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors[0], Is.EqualTo("You cannot book a farm animal together with a lion or polar bear."));
        }

        [Test]
        public void ValidateBooking_PenguinOnWeekend_ReturnsError()
        {
            // Arrange
            var selectedAnimals = new List<Animal>
            {
                new Animal { Name = "Pinguïn", Type = "Pinguïn" }
            };
            int maxAnimals = 5;
            var eventDate = new DateTime(2025, 1, 25); // Saturday

            // Act
            var errors = BookingValidationService.ValidateBooking(selectedAnimals, maxAnimals, eventDate);

            // Assert
            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors[0], Is.EqualTo("You cannot book a penguin on weekends."));
        }

        [Test]
        public void ValidateBooking_DesertAnimalInWinter_ReturnsError()
        {
            // Arrange
            var selectedAnimals = new List<Animal>
            {
                new Animal { Name = "Camel", Type = "Woestijn" }
            };
            int maxAnimals = 5;
            var eventDate = new DateTime(2025, 1, 15); // January

            // Act
            var errors = BookingValidationService.ValidateBooking(selectedAnimals, maxAnimals, eventDate);

            // Assert
            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors[0], Is.EqualTo("You cannot book a desert animal from October to February."));
        }

        [Test]
        public void ValidateBooking_SnowAnimalInSummer_ReturnsError()
        {
            // Arrange
            var selectedAnimals = new List<Animal>
            {
                new Animal { Name = "Polar Bear", Type = "Sneeuw" }
            };
            int maxAnimals = 5;
            var eventDate = new DateTime(2025, 7, 15); // July

            // Act
            var errors = BookingValidationService.ValidateBooking(selectedAnimals, maxAnimals, eventDate);

            // Assert
            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors[0], Is.EqualTo("You cannot book a snow animal from June to August."));
        }

        [Test]
        public void ValidateBooking_ValidBooking_ReturnsNoErrors()
        {
            // Arrange
            var selectedAnimals = new List<Animal>
            {
                new Animal { Name = "Lion", Type = "Leeuw" },
                new Animal { Name = "Penguin", Type = "Pinguïn" }
            };
            int maxAnimals = 5;
            var eventDate = DateTime.Now;

            // Act
            var errors = BookingValidationService.ValidateBooking(selectedAnimals, maxAnimals, eventDate);

            // Assert
            Assert.That(errors, Is.Empty);
        }
    }
}
