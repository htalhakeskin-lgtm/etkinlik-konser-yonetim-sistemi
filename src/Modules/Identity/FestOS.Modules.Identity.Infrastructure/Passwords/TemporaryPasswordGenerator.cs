using System.Security.Cryptography;
using FestOS.Modules.Identity.Application.Passwords;

namespace FestOS.Modules.Identity.Infrastructure.Passwords;

/// <summary>
/// Sixteen random characters in groups of four, without the look-alikes 0/O and 1/I/L, so it can be read
/// out or typed from a screen (identity §6).
/// </summary>
internal sealed class TemporaryPasswordGenerator : ITemporaryPasswordGenerator
{
    public const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    public string Generate() =>
        string.Join('-', Enumerable.Range(0, 4).Select(_ => RandomNumberGenerator.GetString(Alphabet, 4)));
}
