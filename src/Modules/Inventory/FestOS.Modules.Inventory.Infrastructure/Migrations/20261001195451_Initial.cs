using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FestOS.Modules.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "inventory");

            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                schema: "inventory",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<Guid>(type: "uuid", nullable: false),
                    fingerprint = table.Column<string>(
                        type: "character(64)",
                        fixedLength: true,
                        maxLength: 64,
                        nullable: false
                    ),
                    result = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idempotency_keys", x => new { x.user_id, x.key });
                }
            );

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "inventory",
                columns: table => new
                {
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    handler = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => new { x.message_id, x.handler });
                }
            );

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityAlwaysColumn
                        ),
                    type = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ordering_key = table.Column<Guid>(type: "uuid", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    trace_parent = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    dispatched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    failed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "warehouses",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_search = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("pk_warehouses", x => x.id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_idempotency_keys_created_at",
                schema: "inventory",
                table: "idempotency_keys",
                column: "created_at"
            );

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_ordering_key_sequence",
                schema: "inventory",
                table: "outbox_messages",
                columns: new[] { "ordering_key", "sequence" },
                filter: "dispatched_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_sequence",
                schema: "inventory",
                table: "outbox_messages",
                column: "sequence",
                filter: "dispatched_at IS NULL AND failed_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ux_warehouses_name",
                schema: "inventory",
                table: "warehouses",
                column: "name_search",
                unique: true
            );

            // The module role works only in its own schema (database §4). Warehouses are never deleted, only deactivated.
            migrationBuilder.Sql("GRANT USAGE ON SCHEMA inventory TO festos_inventory;");
            migrationBuilder.Sql("GRANT SELECT, INSERT, UPDATE ON inventory.warehouses TO festos_inventory;");
            migrationBuilder.Sql(
                "GRANT SELECT, INSERT, UPDATE, DELETE ON inventory.outbox_messages, inventory.inbox_messages, inventory.idempotency_keys TO festos_inventory;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("REVOKE USAGE ON SCHEMA inventory FROM festos_inventory;");

            migrationBuilder.DropTable(name: "idempotency_keys", schema: "inventory");

            migrationBuilder.DropTable(name: "inbox_messages", schema: "inventory");

            migrationBuilder.DropTable(name: "outbox_messages", schema: "inventory");

            migrationBuilder.DropTable(name: "warehouses", schema: "inventory");
        }
    }
}
