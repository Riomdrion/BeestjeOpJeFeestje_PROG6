using BeestjeOpJeFeestje_PROG6.data.Models;

namespace BeestjeOpJeFeestje_PROG6.ViewModel;

public class BookingVM
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int AnimalId { get; set; }
    public Animal Animal { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}