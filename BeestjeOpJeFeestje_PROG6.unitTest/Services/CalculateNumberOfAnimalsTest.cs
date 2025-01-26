using NUnit.Framework;
using BeestjeOpJeFeestje_PROG6.Services;

namespace BeestjeOpJeFeestje_PROG6.unitTest.Services
{
    [TestFixture]
    public class CalculateNumberOfAnimalsTests
    {
        [Test]
        public void GetMaxAnimals_SilverCard_ReturnsFour()
        {
            // Act
            var result = CalculateNumberOfAnimals.GetMaxAnimals("silver");

            // Assert
            Assert.That(result, Is.EqualTo(4));
        }

        [Test]
        public void GetMaxAnimals_GoldCard_ReturnsMaxValue()
        {
            // Act
            var result = CalculateNumberOfAnimals.GetMaxAnimals("gold");

            // Assert
            Assert.That(result, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void GetMaxAnimals_PlatinumCard_ReturnsMaxValue()
        {
            // Act
            var result = CalculateNumberOfAnimals.GetMaxAnimals("platinum");

            // Assert
            Assert.That(result, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void GetMaxAnimals_DefaultCard_ReturnsThree()
        {
            // Act
            var result = CalculateNumberOfAnimals.GetMaxAnimals("bronze");

            // Assert
            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void GetBookingVipStatus_PlatinumCard_ReturnsTrue()
        {
            // Act
            var result = CalculateNumberOfAnimals.GetBookingVipStatus("platinum");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetBookingVipStatus_SilverCard_ReturnsFalse()
        {
            // Act
            var result = CalculateNumberOfAnimals.GetBookingVipStatus("silver");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetBookingVipStatus_GoldCard_ReturnsFalse()
        {
            // Act
            var result = CalculateNumberOfAnimals.GetBookingVipStatus("gold");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetBookingVipStatus_DefaultCard_ReturnsFalse()
        {
            // Act
            var result = CalculateNumberOfAnimals.GetBookingVipStatus("bronze");

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
