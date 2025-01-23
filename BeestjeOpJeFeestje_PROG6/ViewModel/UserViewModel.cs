namespace BeestjeOpJeFeestje_PROG6.ViewModel
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? Card { get; set; }
        public string Role { get; set; }
    }
}
