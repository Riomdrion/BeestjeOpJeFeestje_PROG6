using Microsoft.AspNetCore.Identity;

public class PasswordService
{
    private readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();

    // Methode om een wachtwoord te hashen
    public string HashPassword(string plainPassword)
    {
        return _passwordHasher.HashPassword(null, plainPassword);
    }

    // Methode om een wachtwoord te verifiëren
    public bool VerifyPassword(string hashedPassword, string plainPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, plainPassword);
        return result == PasswordVerificationResult.Success;
    }
}