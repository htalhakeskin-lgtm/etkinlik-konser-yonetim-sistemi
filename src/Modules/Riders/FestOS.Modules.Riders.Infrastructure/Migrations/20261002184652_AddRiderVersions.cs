using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Riders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRiderVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rider_versions",
                schema: "riders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_by_name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rider_versions", x => x.id);
                    table.ForeignKey(
                        name: "fk_rider_versions_riders_rider_id",
                        column: x => x.rider_id,
                        principalSchema: "riders",
                        principalTable: "riders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "rider_lines",
                schema: "riders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_key = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    model_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    flexibility = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    rider_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rider_lines", x => x.id);
                    table.CheckConstraint("ck_rider_lines_flexibility", "(model_id IS NULL) = (flexibility IS NULL)");
                    table.CheckConstraint("ck_rider_lines_flexibility_enum", "flexibility IN ('flexible', 'required')");
                    table.CheckConstraint("ck_rider_lines_quantity", "quantity >= 1");
                    table.CheckConstraint("ck_rider_lines_target", "num_nonnulls(model_id, category_id, kit_id) = 1");
                    table.ForeignKey(
                        name: "fk_rider_lines_rider_versions_rider_version_id",
                        column: x => x.rider_version_id,
                        principalSchema: "riders",
                        principalTable: "rider_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "rider_equivalent_models",
                schema: "riders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    rider_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rider_equivalent_models", x => x.id);
                    table.ForeignKey(
                        name: "fk_rider_equivalent_models_rider_lines_rider_line_id",
                        column: x => x.rider_line_id,
                        principalSchema: "riders",
                        principalTable: "rider_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_rider_equivalent_models_model_id",
                schema: "riders",
                table: "rider_equivalent_models",
                column: "model_id"
            );

            migrationBuilder.CreateIndex(
                name: "ux_rider_equivalent_models_model",
                schema: "riders",
                table: "rider_equivalent_models",
                columns: new[] { "rider_line_id", "model_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_rider_lines_category_id",
                schema: "riders",
                table: "rider_lines",
                column: "category_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_rider_lines_kit_id",
                schema: "riders",
                table: "rider_lines",
                column: "kit_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_rider_lines_model_id",
                schema: "riders",
                table: "rider_lines",
                column: "model_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_rider_lines_rider_version_id",
                schema: "riders",
                table: "rider_lines",
                column: "rider_version_id"
            );

            migrationBuilder.CreateIndex(
                name: "ux_rider_versions_number",
                schema: "riders",
                table: "rider_versions",
                columns: new[] { "rider_id", "number" },
                unique: true
            );

            migrationBuilder.Sql(
                "GRANT SELECT, INSERT ON riders.rider_versions, riders.rider_lines, riders.rider_equivalent_models TO festos_riders;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "rider_equivalent_models", schema: "riders");

            migrationBuilder.DropTable(name: "rider_lines", schema: "riders");

            migrationBuilder.DropTable(name: "rider_versions", schema: "riders");
        }
    }
}
