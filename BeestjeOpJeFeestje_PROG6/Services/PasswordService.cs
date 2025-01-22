using Microsoft.AspNetCore.Identity;

namespace BeestjeOpJeFeestje_PROG6.Services
{
    public static class PasswordService
    {
        private static readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();

        // Methode om een wachtwoord te hashen
        public static string HashPassword(string plainPassword)
        {
            return _passwordHasher.HashPassword(null, plainPassword);
        }

        // Methode om een wachtwoord te verifiëren
        public static bool VerifyPassword(string hashedPassword, string plainPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, plainPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}