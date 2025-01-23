using System.ComponentModel.DataAnnotations;

namespace BeestjeOpJeFeestje_PROG6.ViewModel
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        [Display(Name = "Password")]
        public string Password { get; set; }
        public string? Card { get; set; }
        public string Role { get; set; }
    }
}
