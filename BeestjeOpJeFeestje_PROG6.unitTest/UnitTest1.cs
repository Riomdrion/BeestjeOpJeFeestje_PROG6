using System.Security.Cryptography;

namespace BeestjeOpJeFeestje_PROG6.unitTest;

public class Tests
{
    // Test to ensure that encrypting and decrypting returns the original password
    [Test]
    public void EncryptPassword_DecryptPassword_ReturnsOriginalPassword()
    {
        // Arrange
        string originalPassword = "MySecretPassword123";

        // Act
        string encryptedPassword = PasswordService.EncryptPassword(originalPassword);
        string decryptedPassword = PasswordService.DecryptPassword(encryptedPassword);

        // Assert
        Assert.That(decryptedPassword, Is.EqualTo(originalPassword), "The decrypted password should match the original password.");
    }

    // Test to ensure the encrypted password is not the same as the original password
    [Test]
    public void EncryptPassword_ReturnsEncryptedPassword()
    {
        // Arrange
        string password = "TestPassword123";

        // Act
        string encryptedPassword = PasswordService.EncryptPassword(password);

        // Assert
        Assert.That(encryptedPassword, Is.Not.EqualTo(password), "The encrypted password should be different from the original password.");
    }

    // Test to ensure decryption of an invalid encrypted password throws an exception
    [Test]
    public void DecryptPassword_WithInvalidData_ThrowsFormatException()
    {
        // Arrange
        string invalidEncryptedPassword = "InvalidEncryptedPassword123";

        // Act & Assert
        var ex = Assert.Throws<FormatException>(() => PasswordService.DecryptPassword(invalidEncryptedPassword));
        Assert.That(ex.Message, Does.Contain("The input is not in a valid Base64 format"));
    }
}