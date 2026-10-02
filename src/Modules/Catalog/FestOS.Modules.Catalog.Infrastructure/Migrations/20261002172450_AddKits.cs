using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kits",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    name_search = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("pk_kits", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "kit_lines",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sub_kit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    kit_id = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kit_lines", x => x.id);
                    table.CheckConstraint("ck_kit_lines_quantity", "quantity >= 1");
                    table.CheckConstraint("ck_kit_lines_target", "num_nonnulls(model_id, sub_kit_id) = 1");
                    table.ForeignKey(
                        name: "fk_kit_lines_equipment_models_model_id",
                        column: x => x.model_id,
                        principalSchema: "catalog",
                        principalTable: "equipment_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_kit_lines_kits_kit_id",
                        column: x => x.kit_id,
                        principalSchema: "catalog",
                        principalTable: "kits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_kit_lines_kits_sub_kit_id",
                        column: x => x.sub_kit_id,
                        principalSchema: "catalog",
                        principalTable: "kits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_kit_lines_kit_id",
                schema: "catalog",
                table: "kit_lines",
                column: "kit_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_kit_lines_model_id",
                schema: "catalog",
                table: "kit_lines",
                column: "model_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_kit_lines_sub_kit_id",
                schema: "catalog",
                table: "kit_lines",
                column: "sub_kit_id"
            );

            migrationBuilder.CreateIndex(
                name: "ux_kits_name",
                schema: "catalog",
                table: "kits",
                column: "name_search",
                unique: true
            );

            migrationBuilder.Sql("GRANT SELECT, INSERT, UPDATE ON catalog.kits TO festos_catalog;");
            migrationBuilder.Sql("GRANT SELECT, INSERT, UPDATE, DELETE ON catalog.kit_lines TO festos_catalog;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "kit_lines", schema: "catalog");

            migrationBuilder.DropTable(name: "kits", schema: "catalog");
        }
    }
}
