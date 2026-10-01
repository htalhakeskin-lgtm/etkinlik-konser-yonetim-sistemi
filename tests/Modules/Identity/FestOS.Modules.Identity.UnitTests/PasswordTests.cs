using System.Text.RegularExpressions;
using FestOS.Modules.Identity.Application;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Identity.Infrastructure;
using FestOS.Modules.Identity.Infrastructure.Passwords;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FestOS.Modules.Identity.UnitTests;

public sealed partial class PasswordTests
{
    [Fact]
    [Trait("Rule", "BR-SYS-006")]
    public void TemporaryPassword_IsSixteenUnambiguousCharactersInGroupsOfFour()
    {
        var generator = new TemporaryPasswordGenerator();

        string first = generator.Generate();

        TemporaryPasswordFormat().IsMatch(first).ShouldBeTrue(first);
        string.Equals(first, generator.Generate(), StringComparison.Ordinal).ShouldBeFalse();
    }

    [Fact]
    public void Hash_VerifiesTheSamePasswordOnly()
    {
        var hasher = new PasswordHasher();

        string hash = hasher.Hash("doğru at pil zımba");

        hasher.Verify(hash, "doğru at pil zımba", out bool needsRehash).ShouldBeTrue();
        needsRehash.ShouldBeFalse();
        hasher.Verify(hash, "doğru at pil zimba", out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Verify_WithoutAHash_NeverMatches(string? hash)
    {
        new PasswordHasher().Verify(hash, "", out bool needsRehash).ShouldBeFalse();
        needsRehash.ShouldBeFalse();
    }

    [Fact]
    public void Options_OutsideTheirLimits_AreRejectedAtStartup()
    {
        var validator = new IdentityModuleOptionsValidator();

        validator.Validate(null, new IdentityModuleOptions()).Succeeded.ShouldBeTrue();
        validator.Validate(null, new IdentityModuleOptions { LockoutMaxFailedAttempts = 0 }).Failed.ShouldBeTrue();
        validator
            .Validate(null, new IdentityModuleOptions { SessionIdleTimeout = TimeSpan.Zero })
            .Failed.ShouldBeTrue();
    }

    [Fact]
    public void HashMadeWithFewerIterations_StillVerifiesButAsksToBeReplaced()
    {
        var older = new PasswordHasher<User>(Options.Create(new PasswordHasherOptions { IterationCount = 100_000 }));
        string oldHash = older.HashPassword(user: null!, "eski şifre ama uzun");

        new PasswordHasher().Verify(oldHash, "eski şifre ama uzun", out bool needsRehash).ShouldBeTrue();
        needsRehash.ShouldBeTrue();
    }

    [GeneratedRegex("^[A-HJKMNP-Z2-9]{4}(-[A-HJKMNP-Z2-9]{4}){3}$", RegexOptions.ExplicitCapture, 1000)]
    private static partial Regex TemporaryPasswordFormat();
}
