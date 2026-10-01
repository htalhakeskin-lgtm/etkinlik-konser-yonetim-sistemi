using FestOS.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Identity.Infrastructure.Sessions;

internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");
        builder.Property(session => session.KeyHash).HasMaxLength(64).IsFixedLength();
        builder.HasIndex(session => session.KeyHash).IsUnique().HasDatabaseName("ux_sessions_key_hash");

        // Not part of the user aggregate, so no cascade: users are never deleted, and a cascade would make the
        // save steps treat a session as a child of its user (building-blocks §4).
        builder.HasOne<User>().WithMany().HasForeignKey(session => session.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
