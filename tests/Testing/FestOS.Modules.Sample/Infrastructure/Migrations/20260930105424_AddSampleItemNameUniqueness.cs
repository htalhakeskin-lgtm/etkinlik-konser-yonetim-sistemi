using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Sample.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSampleItemNameUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ux_sample_items_name",
                schema: "sample",
                table: "sample_items",
                column: "name",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "ux_sample_items_name", schema: "sample", table: "sample_items");
        }
    }
}
