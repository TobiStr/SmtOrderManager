using System.Security.Cryptography;

namespace SmtOrderManager.Domain.Primitives;

/// <summary>
/// Generates UUIDv7 (timestamp-based) identifiers.
/// UUIDv7 provides time-ordered values that are more database-friendly than random UUIDs.
/// </summary>
public static class UuidV7Generator
{
    /// <summary>
    /// Generates a new UUIDv7 identifier.
    /// Format: 48 bits of timestamp + 12 bits of randomness + 2 bits of variant + 62 bits of randomness
    /// </summary>
    /// <returns>A new UUIDv7 identifier.</returns>
    public static Guid Generate()
    {
        return Generate(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Generates a UUIDv7 identifier with a specific timestamp.
    /// Useful for testing purposes.
    /// </summary>
    /// <param name="timestamp">The timestamp to use for the UUID.</param>
    /// <returns>A new UUIDv7 identifier.</returns>
    public static Guid Generate(DateTimeOffset timestamp)
    {
        // Get Unix timestamp in milliseconds (48 bits)
        long unixMs = timestamp.ToUnixTimeMilliseconds();

        // Create a 16-byte array for the UUID
        byte[] guidBytes = new byte[16];

        // Fill with random bytes
        RandomNumberGenerator.Fill(guidBytes);

        // Set timestamp (first 48 bits / 6 bytes)
        guidBytes[0] = (byte)((unixMs >> 40) & 0xFF);
        guidBytes[1] = (byte)((unixMs >> 32) & 0xFF);
        guidBytes[2] = (byte)((unixMs >> 24) & 0xFF);
        guidBytes[3] = (byte)((unixMs >> 16) & 0xFF);
        guidBytes[4] = (byte)((unixMs >> 8) & 0xFF);
        guidBytes[5] = (byte)(unixMs & 0xFF);

        // Set version (4 bits) to 7 (0111 in binary)
        // This is in byte 6, upper 4 bits
        guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x70);

        // Set variant (2 bits) to 10 (RFC 4122)
        // This is in byte 8, upper 2 bits
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        return new Guid(guidBytes);
    }
}
