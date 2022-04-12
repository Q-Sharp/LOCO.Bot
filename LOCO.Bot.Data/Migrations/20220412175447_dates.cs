using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LOCO.Bot.Data.Migrations;

public partial class dates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "EndDate",
            table: "MemberGuess",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "ResultDate",
            table: "MemberGuess",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "StartDate",
            table: "MemberGuess",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Restart",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Guild = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                Channel = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Restart", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Restart");

        migrationBuilder.DropColumn(
            name: "EndDate",
            table: "MemberGuess");

        migrationBuilder.DropColumn(
            name: "ResultDate",
            table: "MemberGuess");

        migrationBuilder.DropColumn(
            name: "StartDate",
            table: "MemberGuess");
    }
}
