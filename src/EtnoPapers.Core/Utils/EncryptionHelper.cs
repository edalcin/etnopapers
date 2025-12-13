using System.Security.Cryptography;
using System.Text;

namespace EtnoPapers.Core.Utils;

/// <summary>
/// Windows DPAPI-based encryption helper for sensitive data like API keys.
/// Uses DataProtectionScope.CurrentUser for per-user encryption.
/// </summary>
public static class EncryptionHelper
{
    /// <summary>
    /// Encrypts sensitive data using Windows DPAPI.
    /// Encryption is scoped to the current user.
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <returns>Base64-encoded encrypted data</returns>
    /// <throws>ArgumentException if plainText is null or empty</throws>
    /// <throws>CryptographicException if encryption fails</throws>
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty.", nameof(plainText));

        try
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (CryptographicException ex)
        {
            throw new CryptographicException("Failed to encrypt data.", ex);
        }
    }

    /// <summary>
    /// Decrypts data encrypted with the Encrypt method.
    /// </summary>
    /// <param name="encryptedBase64">Base64-encoded encrypted data</param>
    /// <returns>The decrypted plain text</returns>
    /// <throws>ArgumentException if encryptedBase64 is null or empty</throws>
    /// <throws>FormatException if encryptedBase64 is not valid Base64</throws>
    /// <throws>CryptographicException if decryption fails</throws>
    public static string Decrypt(string encryptedBase64)
    {
        if (string.IsNullOrEmpty(encryptedBase64))
            throw new ArgumentException("Encrypted text cannot be null or empty.", nameof(encryptedBase64));

        try
        {
            var encryptedBytes = Convert.FromBase64String(encryptedBase64);
            var plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (FormatException ex)
        {
            throw new FormatException("Invalid Base64 format for encrypted data.", ex);
        }
        catch (CryptographicException ex)
        {
            throw new CryptographicException("Failed to decrypt data. It may have been encrypted with a different user account.", ex);
        }
    }
}
