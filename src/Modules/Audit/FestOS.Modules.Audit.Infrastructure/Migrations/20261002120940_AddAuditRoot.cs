using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Audit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditRoot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_audit_entries_entity_type_entity_id_occurred_at",
                schema: "audit",
                table: "audit_entries"
            );

            migrationBuilder.AddColumn<Guid>(
                name: "root_id",
                schema: "audit",
                table: "audit_entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<string>(
                name: "root_type",
                schema: "audit",
                table: "audit_entries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: ""
            );

            // The rows written so far all belong to aggregate roots, so each is its own root (parties MD-01).
            migrationBuilder.Sql("UPDATE audit.audit_entries SET root_type = entity_type, root_id = entity_id;");

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_root_id_occurred_at_id",
                schema: "audit",
                table: "audit_entries",
                columns: new[] { "root_id", "occurred_at", "id" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_audit_entries_root_id_occurred_at_id",
                schema: "audit",
                table: "audit_entries"
            );

            migrationBuilder.DropColumn(name: "root_id", schema: "audit", table: "audit_entries");

            migrationBuilder.DropColumn(name: "root_type", schema: "audit", table: "audit_entries");

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_entity_type_entity_id_occurred_at",
                schema: "audit",
                table: "audit_entries",
                columns: new[] { "entity_type", "entity_id", "occurred_at" }
            );
        }
    }
}
