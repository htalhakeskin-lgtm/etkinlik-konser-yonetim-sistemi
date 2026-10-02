using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "equipment_models",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    brand_name_search = table.Column<string>(
                        type: "character varying(301)",
                        maxLength: 301,
                        nullable: false
                    ),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tracking_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    weight_kilograms = table.Column<decimal>(
                        type: "numeric(10,3)",
                        precision: 10,
                        scale: 3,
                        nullable: true
                    ),
                    power_watts = table.Column<int>(type: "integer", nullable: true),
                    transport_volume_cubic_meters = table.Column<decimal>(
                        type: "numeric(8,3)",
                        precision: 8,
                        scale: 3,
                        nullable: true
                    ),
                    has_stock = table.Column<bool>(type: "boolean", nullable: false),
                    deactivated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deactivated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipment_models", x => x.id);
                    table.CheckConstraint("ck_equipment_models_power_watts", "power_watts > 0");
                    table.CheckConstraint(
                        "ck_equipment_models_tracking_type_enum",
                        "tracking_type IN ('bulk', 'serialized')"
                    );
                    table.CheckConstraint(
                        "ck_equipment_models_transport_volume_cubic_meters",
                        "transport_volume_cubic_meters > 0"
                    );
                    table.CheckConstraint("ck_equipment_models_weight_kilograms", "weight_kilograms > 0");
                    table.ForeignKey(
                        name: "fk_equipment_models_equipment_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "catalog",
                        principalTable: "equipment_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_equipment_models_category_id",
                schema: "catalog",
                table: "equipment_models",
                column: "category_id"
            );

            migrationBuilder
                .CreateIndex(
                    name: "ix_equipment_models_search",
                    schema: "catalog",
                    table: "equipment_models",
                    column: "brand_name_search"
                )
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ux_equipment_models_brand_name",
                schema: "catalog",
                table: "equipment_models",
                column: "brand_name_search",
                unique: true
            );

            migrationBuilder.Sql("GRANT SELECT, INSERT, UPDATE ON catalog.equipment_models TO festos_catalog;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "equipment_models", schema: "catalog");
        }
    }
}
