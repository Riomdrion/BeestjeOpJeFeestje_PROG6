using System.ComponentModel.DataAnnotations;
using BeestjeOpJeFeestje_PROG6.data.Models;

namespace BeestjeOpJeFeestje_PROG6.ViewModel;

public class BookingVM
{
    public int Id { get; set; }
    [Required]
    public User? User { get; set; } = new();
    [Required]
    public List<Animal>? Animals { get; set; } = new();
    public DateTime EventDate { get; set; }
    public bool IsConfirmed { get; set; }
    public int Discount { get; set; }
    public double Price { get; set; }
}