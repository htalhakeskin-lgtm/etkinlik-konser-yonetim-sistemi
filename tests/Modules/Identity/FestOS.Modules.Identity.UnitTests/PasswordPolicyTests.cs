using System.Text;
using FestOS.Modules.Identity.Domain.Users;
using FestOS.Modules.Identity.Infrastructure.Passwords;

namespace FestOS.Modules.Identity.UnitTests;

public sealed class PasswordPolicyTests
{
    [Theory]
    [Trait("Rule", "BR-SYS-007")]
    [InlineData("on-dört-harfli", PasswordProblem.TooShort)]
    [InlineData("on-beş-harfli-x", null)]
    [InlineData("şifre	sekmeli-uzun", PasswordProblem.Whitespace)]
    [InlineData("AYSE@example.com-uzun", PasswordProblem.ContainsEmail)]
    [InlineData("benim-FestOS-şifrem", PasswordProblem.ContainsProductName)]
    public void FindProblem_NamesTheFirstBrokenRule(string password, PasswordProblem? problem) =>
        PasswordPolicy.FindProblem(password, "ayse@example.com", minLength: 15).ShouldBe(problem);

    [Fact]
    [Trait("Rule", "BR-SYS-007")]
    public void FindProblem_CountsCharactersNotCodeUnits()
    {
        PasswordPolicy.FindProblem(string.Concat(Enumerable.Repeat("😀", 15)), "", minLength: 15).ShouldBeNull();
        PasswordPolicy
            .FindProblem(new string('a', PasswordPolicy.MaxLength + 1), "", 15)
            .ShouldBe(PasswordProblem.TooLong);
    }

    [Fact]
    public void Normalize_MakesTheSameTextTypedDifferentlyEqual() =>
        PasswordPolicy.Normalize("doğru").ShouldBe("doğru".Normalize(NormalizationForm.FormC));

    [Theory]
    [Trait("Rule", "BR-SYS-007")]
    [InlineData("1qaz2wsx3edc4rfv", true)]
    [InlineData("1QAZ2WSX3EDC4RFV", true)]
    [InlineData("password", true)]
    [InlineData("mavi-kalem-uzun-yol", false)]
    public void CommonPasswords_AreFoundIgnoringCase(string password, bool common) =>
        new CommonPasswords().Contains(password).ShouldBe(common);
}
