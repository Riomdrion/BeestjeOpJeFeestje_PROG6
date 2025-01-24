using BeestjeOpJeFeestje_PROG6.data.Models;

namespace BeestjeOpJeFeestje_PROG6.Services
{
    public static class PriceAndDiscountCalculator
    {
        private static readonly Random Random = new Random();

        public static double CalculatePrice(Booking booking, int discount)
        { 
            double finalPrice = booking.Price * (double)(1 - discount / 100);
            return finalPrice;
        }

        public static int CalculateDiscount(Booking booking, string cardType)
        {
            int discount = 0;

            // 1. 10% korting bij 3 dieren van hetzelfde type
            var groupedAnimals = booking.Animals.GroupBy(a => a.Type);
            if (groupedAnimals.Any(g => g.Count() >= 3))
            {
                discount += 10;
            }

            // 2. 1/6 kans op 50% korting als er een dier met naam 'Eend' is
            if (booking.Animals.Any(a => a.Name.Equals("Eend", StringComparison.OrdinalIgnoreCase)) &&
                Random.Next(1, 7) == 1)
            {
                discount += 50;
            }

            // 3. 15% korting op maandag of dinsdag
            if (booking.EventDate.DayOfWeek == DayOfWeek.Monday || booking.EventDate.DayOfWeek == DayOfWeek.Tuesday)
            {
                discount += 15;
            }

            // 4. 2% korting extra per letter (A, B, C, etc.) in de naam van een dier
            foreach (var animal in booking.Animals)
            {
                var name = animal.Name.ToUpper();
                for (char letter = 'A'; letter <= 'Z'; letter++)
                {
                    if (name.Contains(letter))
                    {
                        discount += 2;
                    }
                }
            }

            // 5. 10% korting voor klanten met een klantenkaart
            if (cardType != "geen")
            {
                discount += 10;
            }

            // 6. Maximaal 60% korting
            discount = Math.Min(discount, 60);

            return discount;
        }
    }
}