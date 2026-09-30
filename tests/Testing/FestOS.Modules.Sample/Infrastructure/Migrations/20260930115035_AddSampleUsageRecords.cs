using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Sample.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSampleUsageRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sample_usage_records",
                schema: "sample",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sample_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sample_usage_records", x => x.id);
                }
            );

            migrationBuilder.Sql(
                "GRANT SELECT, INSERT, UPDATE, DELETE ON sample.sample_usage_records TO festos_sample;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "sample_usage_records", schema: "sample");
        }
    }
}
