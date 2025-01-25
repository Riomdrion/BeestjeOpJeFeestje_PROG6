﻿using System.Security.Cryptography;
using System.Text;

namespace BeestjeOpJeFeestje_PROG6.unitTest;

public class PasswordServcieTest
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

    [Test]
    public void DecryptPassword_WithCorruptData_ThrowsCryptographicException()
    {
        // Arrange
        string corruptEncryptedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("RandomString"));

        // Act & Assert
        var ex = Assert.Throws<CryptographicException>(() => PasswordService.DecryptPassword(corruptEncryptedPassword));
        Assert.That(ex.Message, Does.Contain("Padding is invalid") // May vary based on the decryption failure reason
                          .Or.Contain("The input data is not a complete block"));
    }
}