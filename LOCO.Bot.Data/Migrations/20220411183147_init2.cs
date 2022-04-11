using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LOCO.Bot.Data.Migrations
{
    public partial class init2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GuessingsPossible",
                table: "Setting",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuessingsPossible",
                table: "Setting");
        }
    }
}
