using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FestOS.Modules.Riders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "riders");

            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                schema: "riders",
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
                schema: "riders",
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
                schema: "riders",
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
                name: "productions",
                schema: "riders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    artist_party_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    name_search = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(
                        type: "character varying(2000)",
                        maxLength: 2000,
                        nullable: true
                    ),
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
                    table.PrimaryKey("pk_productions", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "riders",
                schema: "riders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    production_id = table.Column<Guid>(type: "uuid", nullable: true),
                    latest_version_number = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_riders", x => x.id);
                    table.CheckConstraint("ck_riders_source", "source = 'production' AND production_id IS NOT NULL");
                    table.CheckConstraint("ck_riders_source_enum", "source IN ('production')");
                    table.ForeignKey(
                        name: "fk_riders_productions_production_id",
                        column: x => x.production_id,
                        principalSchema: "riders",
                        principalTable: "productions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_idempotency_keys_created_at",
                schema: "riders",
                table: "idempotency_keys",
                column: "created_at"
            );

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_ordering_key_sequence",
                schema: "riders",
                table: "outbox_messages",
                columns: new[] { "ordering_key", "sequence" },
                filter: "dispatched_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_sequence",
                schema: "riders",
                table: "outbox_messages",
                column: "sequence",
                filter: "dispatched_at IS NULL AND failed_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ux_productions_artist_name",
                schema: "riders",
                table: "productions",
                columns: new[] { "artist_party_id", "name_search" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ux_riders_production_id",
                schema: "riders",
                table: "riders",
                column: "production_id",
                unique: true
            );

            migrationBuilder.Sql("GRANT USAGE ON SCHEMA riders TO festos_riders;");
            migrationBuilder.Sql("GRANT SELECT, INSERT, UPDATE ON riders.productions, riders.riders TO festos_riders;");
            migrationBuilder.Sql(
                "GRANT SELECT, INSERT, UPDATE, DELETE ON riders.outbox_messages, riders.inbox_messages, riders.idempotency_keys TO festos_riders;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("REVOKE USAGE ON SCHEMA riders FROM festos_riders;");

            migrationBuilder.DropTable(name: "idempotency_keys", schema: "riders");

            migrationBuilder.DropTable(name: "inbox_messages", schema: "riders");

            migrationBuilder.DropTable(name: "outbox_messages", schema: "riders");

            migrationBuilder.DropTable(name: "riders", schema: "riders");

            migrationBuilder.DropTable(name: "productions", schema: "riders");
        }
    }
}
