using BeestjeOpJeFeestje_PROG6.data.Models;

namespace BeestjeOpJeFeestje_PROG6.ViewModel
{
    public class StepTwoVM
    {
        public List<Animal> AvailableAnimals { get; set; }  = new List<Animal>();
        public int CanBook { get; set; }
        public bool CanBookVip { get; set; }
    }
}