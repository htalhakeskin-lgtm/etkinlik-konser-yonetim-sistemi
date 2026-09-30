using FestOS.Modules.Sample.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Sample.Infrastructure;

internal sealed class SampleUsageRecordConfiguration : IEntityTypeConfiguration<SampleUsageRecord>
{
    public void Configure(EntityTypeBuilder<SampleUsageRecord> builder) => builder.ToTable("sample_usage_records");
}
