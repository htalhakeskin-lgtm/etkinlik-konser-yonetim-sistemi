using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Audit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "audit");

            migrationBuilder.CreateTable(
                name: "audit_entries",
                schema: "audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    module = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    changes = table.Column<string>(type: "jsonb", nullable: false),
                    trace_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_entries", x => x.id);
                    table.CheckConstraint(
                        "ck_audit_entries_action_enum",
                        "action IN ('created', 'deleted', 'statusChanged', 'updated')"
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_entity_type_entity_id_occurred_at",
                schema: "audit",
                table: "audit_entries",
                columns: new[] { "entity_type", "entity_id", "occurred_at" }
            );

            // Every module role may only add entries; the Audit role may only read them. Nobody changes
            // or deletes the history (database §4, §14.2).
            migrationBuilder.Sql("GRANT USAGE ON SCHEMA audit TO festos_audit, festos_audit_writer;");
            migrationBuilder.Sql("GRANT INSERT ON audit.audit_entries TO festos_audit_writer;");
            migrationBuilder.Sql("GRANT SELECT ON audit.audit_entries TO festos_audit;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("REVOKE USAGE ON SCHEMA audit FROM festos_audit, festos_audit_writer;");

            migrationBuilder.DropTable(name: "audit_entries", schema: "audit");
        }
    }
}
