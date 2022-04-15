using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LOCO.Bot.Data.Migrations
{
    public partial class u2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "MemberGuess",
                newName: "Guess");

            migrationBuilder.RenameColumn(name: "Guess", "Guess", "GuessAmount");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "Guess",
                newName: "MemberGuess");

            migrationBuilder.RenameColumn(name: "GuessAmount", "MemberGuess", "Guess");
        }
    }
}
