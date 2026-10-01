using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Audit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActorName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "actor_name",
                schema: "audit",
                table: "audit_entries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_actor_id_occurred_at",
                schema: "audit",
                table: "audit_entries",
                columns: new[] { "actor_id", "occurred_at" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_occurred_at_id",
                schema: "audit",
                table: "audit_entries",
                columns: new[] { "occurred_at", "id" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_audit_entries_actor_id_occurred_at",
                schema: "audit",
                table: "audit_entries"
            );

            migrationBuilder.DropIndex(
                name: "ix_audit_entries_occurred_at_id",
                schema: "audit",
                table: "audit_entries"
            );

            migrationBuilder.DropColumn(name: "actor_name", schema: "audit", table: "audit_entries");
        }
    }
}
