using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FestOS.Modules.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "identity");

            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                schema: "identity",
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
                schema: "identity",
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
                schema: "identity",
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
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false),
                    failed_login_count = table.Column<int>(type: "integer", nullable: false),
                    locked_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    roles = table.Column<string[]>(type: "text[]", nullable: false),
                    warehouse_ids = table.Column<Guid[]>(type: "uuid[]", nullable: false),
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
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint(
                        "ck_users_roles_enum",
                        "roles <@ ARRAY['bookingManager', 'generalManager', 'systemAdministrator', 'technicalManager', 'warehouseManager']::text[]"
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_idempotency_keys_created_at",
                schema: "identity",
                table: "idempotency_keys",
                column: "created_at"
            );

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_ordering_key_sequence",
                schema: "identity",
                table: "outbox_messages",
                columns: new[] { "ordering_key", "sequence" },
                filter: "dispatched_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_sequence",
                schema: "identity",
                table: "outbox_messages",
                column: "sequence",
                filter: "dispatched_at IS NULL AND failed_at IS NULL"
            );

            migrationBuilder
                .CreateIndex(name: "ix_users_roles", schema: "identity", table: "users", column: "roles")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder
                .CreateIndex(
                    name: "ix_users_warehouse_ids",
                    schema: "identity",
                    table: "users",
                    column: "warehouse_ids"
                )
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ux_users_email",
                schema: "identity",
                table: "users",
                column: "email",
                unique: true
            );

            // The module role works only in its own schema (database §4). Users are never deleted, only deactivated.
            migrationBuilder.Sql("GRANT USAGE ON SCHEMA identity TO festos_identity;");
            migrationBuilder.Sql("GRANT SELECT, INSERT, UPDATE ON identity.users TO festos_identity;");
            migrationBuilder.Sql(
                "GRANT SELECT, INSERT, UPDATE, DELETE ON identity.outbox_messages, identity.inbox_messages, identity.idempotency_keys TO festos_identity;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("REVOKE USAGE ON SCHEMA identity FROM festos_identity;");

            migrationBuilder.DropTable(name: "idempotency_keys", schema: "identity");

            migrationBuilder.DropTable(name: "inbox_messages", schema: "identity");

            migrationBuilder.DropTable(name: "outbox_messages", schema: "identity");

            migrationBuilder.DropTable(name: "users", schema: "identity");
        }
    }
}
