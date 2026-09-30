using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Audit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxOrderingIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_ordering_key_sequence",
                schema: "audit",
                table: "outbox_messages",
                columns: new[] { "ordering_key", "sequence" },
                filter: "dispatched_at IS NULL"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_ordering_key_sequence",
                schema: "audit",
                table: "outbox_messages"
            );
        }
    }
}
