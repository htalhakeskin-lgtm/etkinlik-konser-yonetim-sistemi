using Microsoft.EntityFrameworkCore;

namespace FestOS.BuildingBlocks.Infrastructure.Idempotency;

/// <summary>Maps the idempotency key table in every module's schema (database §15).</summary>
internal static class IdempotencyModel
{
    public const int FingerprintLength = 64;

    public static void Configure(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<IdempotencyKey>(key =>
        {
            key.ToTable("idempotency_keys");
            key.HasKey(stored => new { stored.UserId, stored.Key });
            key.Property(stored => stored.Fingerprint).HasMaxLength(FingerprintLength).IsFixedLength();
            key.Property(stored => stored.Result).HasColumnType("jsonb");

            // The nightly cleanup deletes by age.
            key.HasIndex(stored => stored.CreatedAt);
        });
}
