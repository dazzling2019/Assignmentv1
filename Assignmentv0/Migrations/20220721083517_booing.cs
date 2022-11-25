using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignmentv1.Migrations
{
    public partial class booing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FeteName",
                table: "Fete",
                newName: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Fete",
                newName: "FeteName");
        }
    }
}
