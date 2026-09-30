using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Sample.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSampleItemInternalCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "internal_code",
                schema: "sample",
                table: "sample_items",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "internal_code", schema: "sample", table: "sample_items");
        }
    }
}
