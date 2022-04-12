using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LOCO.Bot.Data.Migrations;

public partial class Init : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MemberGuess",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                MemberName = table.Column<string>(type: "text", nullable: true),
                MemberId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                Guess = table.Column<double>(type: "double precision", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemberGuess", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Setting",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                GuessChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                GuessMemberRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Setting", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "MemberGuess");

        migrationBuilder.DropTable(
            name: "Setting");
    }
}
