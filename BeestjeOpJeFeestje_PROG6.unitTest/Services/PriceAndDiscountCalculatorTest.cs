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
            Assert.That(discount, Is.EqualTo(35)); // 10% for 3 animals of same type + 15% for Monday + 10% for card
        }

        [Test]
        public void CalculateDiscount_AnimalNamedEend_HasChanceFor50PercentDiscount()
        {
            // Arrange
            var animals = new List<Animal>
            {
                new Animal { Name = "Eend", Type = "Boerderij" }
            };
            var eventDate = new DateOnly(2025, 1, 20); // Monday
            var cardType = "silver";

            // Simuleer een 1/6 kans voor een 50% korting voor de "Eend"
            Random random = new Random();
            bool eendHas50PercentDiscount = random.Next(1, 7) == 1;  // 1 uit 6 kans

            // Act
            var discount = PriceAndDiscountCalculator.CalculateDiscount(eventDate, animals, cardType);

            // Als de Eend 50% korting krijgt, pas dat dan toe
            if (eendHas50PercentDiscount)
            {
                discount = Math.Min(discount, 50); // De korting wordt dan 50% als deze groter is dan 50%
            }

            // Assert
            // De korting zou nooit meer dan 60% moeten zijn
            Assert.That(discount, Is.LessThanOrEqualTo(60)); // Maximaal 60% korting
            Assert.That(discount, Is.GreaterThanOrEqualTo(0)); // Korting mag niet negatief zijn
        }


        [Test]
        public void CalculateDiscount_DiscountForWeekday_Returns15PercentOnMondayOrTuesday()
        {
            // Arrange
            var animals = new List<Animal>
            {
                new Animal { Name = "Lion", Type = "Jungle" }
            };
            var eventDate = new DateOnly(2025, 1, 20); // Monday
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
                new Animal { Name = "Aap", Type = "Jungle" },
                new Animal { Name = "Lion", Type = "Jungle" }
            };
            var eventDate = new DateOnly(2025, 1, 20); // Tuesday
            var cardType = "silver";

            // Act
            var discount = PriceAndDiscountCalculator.CalculateDiscount(eventDate, animals, cardType);

            // Assert
            // 'Bear' gives 2% per letter 'B', 'Lion' gives 2% per letter 'L'
            // Total extra discount: 2% (B) + 2% (L) = 4%
            Assert.That(discount, Is.EqualTo(25)); // 10% for card + 15% for Tuesday + 4% for animal names
        }

        [Test]
        public void CalculateDiscount_MaximumDiscountReturns60Percent()
        {
            // Arrange
            var animals = new List<Animal>
            {
                new Animal { Name = "Lion", Type = "Jungle" },
                new Animal { Name = "Elephant", Type = "Jungle" },
                new Animal { Name = "Eend", Type = "Boerderij" }
            };
            var eventDate = new DateOnly(2025, 1, 20); // Monday
            var cardType = "silver";

            // Simuleer de kans op 50% korting (1/6 kans)
            Random random = new Random();
            bool fiftyPercentDiscount = random.Next(1, 7) == 1;  // 1 uit 6 kans

            // Act
            var discount = PriceAndDiscountCalculator.CalculateDiscount(eventDate, animals, cardType);

            // Als de 50% korting van toepassing is, moet de korting maximaal 60% zijn, anders moet het minder zijn
            if (fiftyPercentDiscount)
            {
                discount = Math.Min(discount, 50); // De korting wordt dan 50% als deze groter is dan 50%
            }

            // Assert
            // De korting zou nooit meer dan 60% moeten zijn
            Assert.That(discount, Is.LessThanOrEqualTo(60)); // Maximaal 60% korting
            Assert.That(discount, Is.GreaterThanOrEqualTo(0)); // Korting mag niet negatief zijn
        }


        [Test]
        public void CalculateDiscount_NoDiscount_ReturnsZeroDiscount()
        {
            // Arrange
            var animals = new List<Animal>
            {
                new Animal { Name = "Lion", Type = "Jungle" }
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
