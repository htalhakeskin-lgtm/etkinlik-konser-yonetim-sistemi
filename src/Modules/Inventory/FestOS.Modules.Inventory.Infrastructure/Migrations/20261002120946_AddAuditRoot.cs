using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditRoot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The change history table belongs to the Audit module; this context maps it without creating it,
            // so only its model snapshot changes (audit §2).
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
