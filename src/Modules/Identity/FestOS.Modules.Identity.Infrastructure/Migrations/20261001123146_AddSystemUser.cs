using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The system user records work no person started (database §9). It has no password, so it
            // can never sign in, and no role.
            migrationBuilder.Sql(
                """
                INSERT INTO identity.users (id, full_name, email, password_hash, must_change_password,
                    failed_login_count, roles, warehouse_ids, version, created_at, created_by, updated_at, updated_by)
                VALUES ('00000000-0000-7000-8000-000000000001', 'Sistem', 'system@festos.invalid', '', false,
                    0, '{}', '{}', 1, now(), '00000000-0000-7000-8000-000000000001',
                    now(), '00000000-0000-7000-8000-000000000001');
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM identity.users WHERE id = '00000000-0000-7000-8000-000000000001';");
        }
    }
}
