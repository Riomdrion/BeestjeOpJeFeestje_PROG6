namespace BeestjeOpJeFeestje_PROG6.data.Models;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int AnimalId { get; set; }
    public Animal Animal { get; set; }
    public DateTime EventDate { get; set; }
    public bool IsConfirmed { get; set; }
}