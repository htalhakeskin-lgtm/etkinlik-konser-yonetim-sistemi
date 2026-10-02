using FestOS.Modules.Riders.Domain.RiderVersions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FestOS.Modules.Riders.Infrastructure;

internal sealed class RiderEquivalentModelConfiguration : IEntityTypeConfiguration<RiderEquivalentModel>
{
    public void Configure(EntityTypeBuilder<RiderEquivalentModel> builder)
    {
        builder.ToTable("rider_equivalent_models");

        // A model is a line's equivalent once (BR-RDR-002); the model is another module's record (DT-03).
        builder
            .HasIndex("RiderLineId", nameof(RiderEquivalentModel.ModelId))
            .IsUnique()
            .HasDatabaseName("ux_rider_equivalent_models_model");
        builder.HasIndex(equivalent => equivalent.ModelId);
    }
}
