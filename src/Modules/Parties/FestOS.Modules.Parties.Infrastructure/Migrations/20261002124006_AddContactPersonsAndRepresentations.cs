using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FestOS.Modules.Parties.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContactPersonsAndRepresentations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "artist_representations",
                schema: "parties",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    artist_id = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_artist_representations", x => x.id);
                    table.ForeignKey(
                        name: "fk_artist_representations_parties_agency_id",
                        column: x => x.agency_id,
                        principalSchema: "parties",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_artist_representations_parties_artist_id",
                        column: x => x.artist_id,
                        principalSchema: "parties",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "organization_contacts",
                schema: "parties",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_contacts", x => x.id);
                    table.ForeignKey(
                        name: "fk_organization_contacts_parties_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "parties",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_organization_contacts_parties_person_id",
                        column: x => x.person_id,
                        principalSchema: "parties",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_artist_representations_agency_id",
                schema: "parties",
                table: "artist_representations",
                column: "agency_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_artist_representations_artist_id",
                schema: "parties",
                table: "artist_representations",
                column: "artist_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_organization_contacts_organization_id",
                schema: "parties",
                table: "organization_contacts",
                column: "organization_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_organization_contacts_person_id",
                schema: "parties",
                table: "organization_contacts",
                column: "person_id"
            );

            // Ties between parties are parts of a party and can be removed (BR-SYS-001).
            migrationBuilder.Sql(
                "GRANT SELECT, INSERT, UPDATE, DELETE ON parties.organization_contacts, parties.artist_representations TO festos_parties;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "artist_representations", schema: "parties");

            migrationBuilder.DropTable(name: "organization_contacts", schema: "parties");
        }
    }
}
