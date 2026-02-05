using System.Security.Cryptography;

namespace WeddingPhotos.Api.Utils;

/// <summary>
/// Generate cryptographically secure GUIDs for gallery access
/// </summary>
public static class SecureGuidGenerator
{
    /// <summary>
    /// Generate cryptographically secure random GUID
    /// Uses 256 bits of entropy for maximum security
    /// </summary>
    /// <returns>URL-safe base64 encoded string (22 characters)</returns>
    public static string Generate()
    {
        // Use cryptographically secure random number generator
        var bytes = RandomNumberGenerator.GetBytes(32); // 256 bits

        // Convert to base64url (URL-safe, no padding)
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")   // Replace + with -
            .Replace("/", "_")   // Replace / with _
            .Replace("=", "")    // Remove padding
            .Substring(0, 22);   // Take first 22 chars (~132 bits entropy)
    }

    /// <summary>
    /// Generate multiple unique GUIDs
    /// </summary>
    /// <param name="count">Number of GUIDs to generate</param>
    /// <returns>List of unique GUIDs</returns>
    public static List<string> GenerateMultiple(int count)
    {
        var guids = new HashSet<string>();

        while (guids.Count < count)
        {
            guids.Add(Generate());
        }

        return guids.ToList();
    }

    /// <summary>
    /// Generate GUID with custom prefix (for easier identification)
    /// </summary>
    /// <param name="prefix">Short prefix (e.g., "wed", "gal")</param>
    /// <returns>Prefixed GUID</returns>
    public static string GenerateWithPrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return Generate();

        var cleanPrefix = new string(prefix
            .Where(char.IsLetterOrDigit)
            .Take(5)
            .ToArray())
            .ToLower();

        return $"{cleanPrefix}_{Generate()}";
    }
}