using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.data.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? Card { get; set; }
    
    public int PhoneNumber { get; set; }
    public ICollection<Booking> Bookings { get; set; }
    public int Role { get; set; }
}