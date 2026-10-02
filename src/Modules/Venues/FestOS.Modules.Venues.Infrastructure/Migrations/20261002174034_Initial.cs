using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FestOS.Modules.Venues.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "venues");

            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                schema: "venues",
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
                schema: "venues",
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
                schema: "venues",
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
                name: "venues",
                schema: "venues",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    name_search = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city_search = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    operator_party_id = table.Column<Guid>(type: "uuid", nullable: true),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    stage_width_meters = table.Column<decimal>(
                        type: "numeric(6,2)",
                        precision: 6,
                        scale: 2,
                        nullable: true
                    ),
                    stage_depth_meters = table.Column<decimal>(
                        type: "numeric(6,2)",
                        precision: 6,
                        scale: 2,
                        nullable: true
                    ),
                    stage_height_meters = table.Column<decimal>(
                        type: "numeric(6,2)",
                        precision: 6,
                        scale: 2,
                        nullable: true
                    ),
                    loading_dock = table.Column<string>(
                        type: "character varying(2000)",
                        maxLength: 2000,
                        nullable: true
                    ),
                    power_capacity_amperes = table.Column<decimal>(
                        type: "numeric(7,2)",
                        precision: 7,
                        scale: 2,
                        nullable: true
                    ),
                    curfew = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    time_zone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
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
                    table.PrimaryKey("pk_venues", x => x.id);
                    table.CheckConstraint("ck_venues_capacity", "capacity > 0");
                    table.CheckConstraint("ck_venues_power_capacity_amperes", "power_capacity_amperes > 0");
                    table.CheckConstraint("ck_venues_stage_depth_meters", "stage_depth_meters > 0");
                    table.CheckConstraint("ck_venues_stage_height_meters", "stage_height_meters > 0");
                    table.CheckConstraint("ck_venues_stage_width_meters", "stage_width_meters > 0");
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_idempotency_keys_created_at",
                schema: "venues",
                table: "idempotency_keys",
                column: "created_at"
            );

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_ordering_key_sequence",
                schema: "venues",
                table: "outbox_messages",
                columns: new[] { "ordering_key", "sequence" },
                filter: "dispatched_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_sequence",
                schema: "venues",
                table: "outbox_messages",
                column: "sequence",
                filter: "dispatched_at IS NULL AND failed_at IS NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_venues_operator_party_id",
                schema: "venues",
                table: "venues",
                column: "operator_party_id"
            );

            migrationBuilder.CreateIndex(
                name: "ux_venues_name_city",
                schema: "venues",
                table: "venues",
                columns: new[] { "city_search", "name_search" },
                unique: true
            );

            migrationBuilder.Sql("GRANT USAGE ON SCHEMA venues TO festos_venues;");
            migrationBuilder.Sql("GRANT SELECT, INSERT, UPDATE ON venues.venues TO festos_venues;");
            migrationBuilder.Sql(
                "GRANT SELECT, INSERT, UPDATE, DELETE ON venues.outbox_messages, venues.inbox_messages, venues.idempotency_keys TO festos_venues;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("REVOKE USAGE ON SCHEMA venues FROM festos_venues;");

            migrationBuilder.DropTable(name: "idempotency_keys", schema: "venues");

            migrationBuilder.DropTable(name: "inbox_messages", schema: "venues");

            migrationBuilder.DropTable(name: "outbox_messages", schema: "venues");

            migrationBuilder.DropTable(name: "venues", schema: "venues");
        }
    }
}
