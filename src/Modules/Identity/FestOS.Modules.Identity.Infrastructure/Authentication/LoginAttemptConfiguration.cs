using FestOS.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Identity.Infrastructure.Authentication;

internal sealed class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        builder.ToTable("login_attempts");
        builder.Property(attempt => attempt.Email).HasMaxLength(EmailAddress.MaxLength);
        builder.HasIndex(attempt => attempt.OccurredAt).HasDatabaseName("ix_login_attempts_occurred_at");

        // Not part of the user aggregate, so no cascade (see the sessions).
        builder.HasOne<User>().WithMany().HasForeignKey(attempt => attempt.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
