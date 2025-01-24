using BeestjeOpJeFeestje_PROG6.data.Models;

namespace BeestjeOpJeFeestje_PROG6.Services
{
    public static class BookingValidationService
    {
        public static List<string> ValidateBooking(List<Animal> selectedAnimalDetails, int canBook, DateTime eventDate)
        {
            var errors = new List<string>();

            // Validatie: Maximaal aantal dieren
            if (selectedAnimalDetails.Count > canBook)
            {
                errors.Add($"You can only book up to {canBook} animals.");
            }

            // Validatie: Geen boerderijdier samen met een leeuw of ijsbeer
            bool hasFarmAnimal = selectedAnimalDetails.Any(a => a.Type == "Boerderijdier");
            bool hasPredator = selectedAnimalDetails.Any(a => a.Type == "Leeuw" || a.Type == "IJsbeer");
            if (hasFarmAnimal && hasPredator)
            {
                errors.Add("You cannot book a farm animal together with a lion or polar bear.");
            }

            // Validatie: Geen pinguïn in het weekend
            if (selectedAnimalDetails.Any(a =>
                    a.Name == "Pinguïn" && (eventDate.DayOfWeek == DayOfWeek.Saturday ||
                                            eventDate.DayOfWeek == DayOfWeek.Sunday)))
            {
                errors.Add("You cannot book a penguin on weekends.");
            }

            // Validatie: Geen woestijndieren in oktober t/m februari
            if (selectedAnimalDetails.Any(a =>
                    a.Type == "Woestijn" && (eventDate.Month >= 10 || eventDate.Month <= 2)))
            {
                errors.Add("You cannot book a desert animal from October to February.");
            }

            // Validatie: Geen sneeuwdieren in juni t/m augustus
            if (selectedAnimalDetails.Any(a =>
                    a.Type == "Sneeuw" && (eventDate.Month >= 6 && eventDate.Month <= 8)))
            {
                errors.Add("You cannot book a snow animal from June to August.");
            }

            return errors;
        }
    }
}