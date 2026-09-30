using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Sample.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "sample");

            migrationBuilder.CreateTable(
                name: "sample_items",
                schema: "sample",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sample_items", x => x.id);
                    table.CheckConstraint("ck_sample_items_status_enum", "status IN ('draft', 'inUse')");
                }
            );

            migrationBuilder.CreateTable(
                name: "sample_item_parts",
                schema: "sample",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    replaces_part_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sample_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sample_item_parts", x => x.id);
                    table.ForeignKey(
                        name: "fk_sample_item_parts_sample_item_parts_replaces_part_id",
                        column: x => x.replaces_part_id,
                        principalSchema: "sample",
                        principalTable: "sample_item_parts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_sample_item_parts_sample_items_sample_item_id",
                        column: x => x.sample_item_id,
                        principalSchema: "sample",
                        principalTable: "sample_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_sample_item_parts_replaces_part_id",
                schema: "sample",
                table: "sample_item_parts",
                column: "replaces_part_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_sample_item_parts_sample_item_id",
                schema: "sample",
                table: "sample_item_parts",
                column: "sample_item_id"
            );

            // The module role reads and writes its tables; the owner keeps the schema (database §4).
            migrationBuilder.Sql("GRANT USAGE ON SCHEMA sample TO festos_sample;");
            migrationBuilder.Sql(
                "GRANT SELECT, INSERT, UPDATE, DELETE ON sample.sample_items, sample.sample_item_parts TO festos_sample;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("REVOKE USAGE ON SCHEMA sample FROM festos_sample;");

            migrationBuilder.DropTable(name: "sample_item_parts", schema: "sample");

            migrationBuilder.DropTable(name: "sample_items", schema: "sample");
        }
    }
}
