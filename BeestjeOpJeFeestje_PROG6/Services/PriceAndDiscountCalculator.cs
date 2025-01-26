using BeestjeOpJeFeestje_PROG6.data.Models;

namespace BeestjeOpJeFeestje_PROG6.Services
{
    public static class PriceAndDiscountCalculator
    {
        private static readonly Random Random = new();

        public static double CalculatePrice(double price, int discount)
        { 
            double finalPrice = price * (1 - discount / 100.0);
            return finalPrice;
        }

        public static int CalculateDiscount(DateOnly eventdate, List<Animal> animals, string cardType)
        {
            int discount = 0;

            // 1. 10% korting bij 3 dieren van hetzelfde type
            var groupedAnimals = animals.GroupBy(a => a.Type);
            if (groupedAnimals.Any(g => g.Count() >= 3))
            {
                discount += 10;
            }

            // 2. 1/6 kans op 50% korting als er een dier met naam 'Eend' is
            if (animals.Any(a => a.Name.Equals("Eend", StringComparison.OrdinalIgnoreCase)) &&
                Random.Next(1, 7) == 1)
            {
                discount += 50;
            }

            // 3. 15% korting op maandag of dinsdag
            if (eventdate.DayOfWeek == DayOfWeek.Monday || eventdate.DayOfWeek == DayOfWeek.Tuesday)
            {
                discount += 15;
            }

            // 4. 2% korting extra per letter (A, B, C, etc.) in de naam van een dier
            foreach (var animal in animals)
            {
                var name = animal.Name.ToUpper();
                char previousLetter = '\0';
                foreach (char letter in name)
                {
                    if (letter >= 'A' && letter <= 'Z' && letter == previousLetter + 1)
                    {
                        discount += 2;
                    }
                    previousLetter = letter;
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