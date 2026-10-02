using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Venues.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVenueEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "venue_equipment",
                schema: "venues",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    validity_start = table.Column<DateOnly>(type: "date", nullable: true),
                    validity_end = table.Column<DateOnly>(type: "date", nullable: true),
                    venue_id = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_venue_equipment", x => x.id);
                    table.CheckConstraint("ck_venue_equipment_quantity", "quantity >= 1");
                    table.CheckConstraint(
                        "ck_venue_equipment_target",
                        "num_nonnulls(model_id, category_id, description) = 1"
                    );
                    table.CheckConstraint(
                        "ck_venue_equipment_validity",
                        "validity_start IS NULL OR validity_end IS NULL OR validity_start < validity_end"
                    );
                    table.ForeignKey(
                        name: "fk_venue_equipment_venues_venue_id",
                        column: x => x.venue_id,
                        principalSchema: "venues",
                        principalTable: "venues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "venue_equipment_unavailabilities",
                schema: "venues",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    venue_equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_venue_equipment_unavailabilities", x => x.id);
                    table.CheckConstraint("ck_venue_equipment_unavailabilities_period", "period_start < period_end");
                    table.CheckConstraint("ck_venue_equipment_unavailabilities_quantity", "quantity >= 1");
                    table.ForeignKey(
                        name: "fk_venue_equipment_unavailabilities_venue_equipment_venue_equi",
                        column: x => x.venue_equipment_id,
                        principalSchema: "venues",
                        principalTable: "venue_equipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_venue_equipment_category_id",
                schema: "venues",
                table: "venue_equipment",
                column: "category_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_venue_equipment_model_id",
                schema: "venues",
                table: "venue_equipment",
                column: "model_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_venue_equipment_venue_id",
                schema: "venues",
                table: "venue_equipment",
                column: "venue_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_venue_equipment_unavailabilities_venue_equipment_id",
                schema: "venues",
                table: "venue_equipment_unavailabilities",
                column: "venue_equipment_id"
            );

            // A line's periods do not overlap (BR-VEN-002); EF cannot describe an exclusion constraint (database §12.1).
            migrationBuilder.Sql(
                "ALTER TABLE venues.venue_equipment_unavailabilities ADD CONSTRAINT ex_venue_equipment_unavailabilities_overlap "
                    + "EXCLUDE USING gist (venue_equipment_id WITH =, daterange(period_start, period_end, '[)') WITH &&);"
            );
            migrationBuilder.Sql(
                "GRANT SELECT, INSERT, UPDATE, DELETE ON venues.venue_equipment, venues.venue_equipment_unavailabilities TO festos_venues;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE venues.venue_equipment_unavailabilities DROP CONSTRAINT ex_venue_equipment_unavailabilities_overlap;"
            );

            migrationBuilder.DropTable(name: "venue_equipment_unavailabilities", schema: "venues");

            migrationBuilder.DropTable(name: "venue_equipment", schema: "venues");
        }
    }
}
