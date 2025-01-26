using BeestjeOpJeFeestje_PROG6.Services;
using BeestjeOpJeFeestje_PROG6.data.Models;

namespace BeestjeOpJeFeestje_PROG6.Tests
{
    [TestFixture]
    public class PriceAndDiscountCalculatorTests
    {
        [Test]
        public void CalculatePrice_ValidPriceAndDiscount_ReturnsCorrectPrice()
        {
            // Arrange
            double price = 100.0;
            int discount = 10;

            // Act
            var finalPrice = PriceAndDiscountCalculator.CalculatePrice(price, discount);

            // Assert
            Assert.That(finalPrice, Is.EqualTo(90.0)); // 100 - (10% of 100) = 90
        }

        [Test]
        public void CalculateDiscount_ThreeAnimalsOfSameType_Apply10PercentDiscount()
        {
            // Arrange
            var animals = new List<Animal>
            {
                new Animal { Name = "Lion", Type = "Jungle" },
                new Animal { Name = "Lion", Type = "Jungle" },
                new Animal { Name = "Lion", Type = "Jungle" }
            };
            var eventDate = new DateOnly(2025, 1, 20); // Monday
            var cardType = "silver";

            // Act
            var discount = PriceAndDiscountCalculator.CalculateDiscount(eventDate, animals, cardType);

            // Assert
            Assert.That(discount, Is.EqualTo(45)); // 10% for 3 animals of same type + 15% for Monday + 10% for card
        }

        [Test]
        public void CalculateDiscount_AnimalNamedEend_HasChanceFor50PercentDiscount()
        {
            // Arrange
            var animals = new List<Animal>
            {
                new Animal { Name = "Eend", Type = "Duck" }
            };
            var eventDate = new DateOnly(2025, 1, 20); // Monday
            var cardType = "silver";

            // Simulate a 1/6 chance for a 50% discount.
            // For testing purposes, we will assume the random roll happens to be 1.
            var discount = PriceAndDiscountCalculator.CalculateDiscount(eventDate, animals, cardType);

            // Act
            // Assert
            Assert.That(discount, Is.EqualTo(60)); // 50% chance + 10% for card type + 15% for Monday
        }

        [Test]
        public void CalculateDiscount_DiscountForWeekday_Returns15PercentOnMondayOrTuesday()
        {
            // Arrange
            var animals = new List<Animal>
            {
                new Animal { Name = "Lion", Type = "Big Cat" }
            };
            var eventDate = new DateOnly(2025, 1, 19); // Monday
            var cardType = "silver";

            // Act
            var discount = PriceAndDiscountCalculator.CalculateDiscount(eventDate, animals, cardType);

            // Assert
            Assert.That(discount, Is.EqualTo(25)); // 15% for Monday + 10% for card type
        }

        [Test]
        public void CalculateDiscount_ExtraDiscountBasedOnAnimalNames_ReturnsCorrectDiscount()
        {
            // Arrange
            var animals = new List<Animal>
            {
                new Animal { Name = "Bear", Type = "Mammal" },
                new Animal { Name = "Lion", Type = "Big Cat" }
            };
            var eventDate = new DateOnly(2025, 1, 20); // Tuesday
            var cardType = "silver";

            // Act
            var discount = PriceAndDiscountCalculator.CalculateDiscount(eventDate, animals, cardType);

            // Assert
            // 'Bear' gives 2% per letter 'B', 'Lion' gives 2% per letter 'L'
            // Total extra discount: 2% (B) + 2% (L) = 4%
            Assert.That(discount, Is.EqualTo(29)); // 10% for card + 15% for Tuesday + 4% for animal names
        }

        [Test]
        public void CalculateDiscount_MaximumDiscountReturns60Percent()
        {
            // Arrange
            var animals = new List<Animal>
            {
                new Animal { Name = "Lion", Type = "Big Cat" },
                new Animal { Name = "Elephant", Type = "Mammal" },
                new Animal { Name = "Eend", Type = "Duck" }
            };
            var eventDate = new DateOnly(2025, 1, 20); // Monday
            var cardType = "silver";

            // Act
            var discount = PriceAndDiscountCalculator.CalculateDiscount(eventDate, animals, cardType);

            // Assert
            // Discount should not exceed 60%
            Assert.That(discount, Is.EqualTo(60)); // 60% is the maximum discount
        }

        [Test]
        public void CalculateDiscount_NoDiscount_ReturnsZeroDiscount()
        {
            // Arrange
            var animals = new List<Animal>
            {
                new Animal { Name = "Lion", Type = "Big Cat" }
            };
            var eventDate = new DateOnly(2025, 1, 15); // Wednesday
            var cardType = "geen";

            // Act
            var discount = PriceAndDiscountCalculator.CalculateDiscount(eventDate, animals, cardType);

            // Assert
            Assert.That(discount, Is.EqualTo(0)); // No discount for no card and no applicable conditions
        }
    }
}
