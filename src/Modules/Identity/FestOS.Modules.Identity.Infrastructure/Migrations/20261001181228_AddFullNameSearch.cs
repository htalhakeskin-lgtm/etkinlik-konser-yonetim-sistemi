using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullNameSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "full_name_search",
                schema: "identity",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: ""
            );

            // The search key of the users that already exist, by the same rule as SearchKey.Of: ICU's
            // Turkish lower case (I to ı, İ to i), then the Turkish letters without their marks.
            migrationBuilder.Sql(
                "UPDATE identity.users SET full_name_search = "
                    + "translate(lower(trim(full_name) COLLATE \"tr-x-icu\"), 'çğıöşü', 'cgiosu');"
            );

            migrationBuilder
                .CreateIndex(
                    name: "ix_users_full_name_search",
                    schema: "identity",
                    table: "users",
                    column: "full_name_search"
                )
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "ix_users_full_name_search", schema: "identity", table: "users");

            migrationBuilder.DropColumn(name: "full_name_search", schema: "identity", table: "users");
        }
    }
}
