using BeestjeOpJeFeestje_PROG6.data.Models;
using System.Collections.Generic;

namespace BeestjeOpJeFeestje_PROG6.ViewModel
{
    public class BookingWizardVM
    {
        public BookingVM Booking { get; set; } 
        public List<Animal> AvailableAnimals { get; set; } 
    }
}