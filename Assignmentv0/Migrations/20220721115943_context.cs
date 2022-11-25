using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignmentv1.Migrations
{
    public partial class context : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Fete",
                columns: new[] { "Id", "EventURL", "FeteTime", "Name", "Synopsis" },
                values: new object[] { 1, null, new DateTime(2015, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Boxing", "Local Boxing event" });

            migrationBuilder.InsertData(
                table: "Room",
                columns: new[] { "Id", "Capacity", "Name", "RoomTitleId" },
                values: new object[] { 1, 150, "Main Hall", "F2-08" });

            migrationBuilder.InsertData(
                table: "Student",
                columns: new[] { "Id", "Email", "Name" },
                values: new object[] { 1, "dl5bbs@bolton.ac.uk", "Daryl Leach" });

            migrationBuilder.InsertData(
                table: "Booking",
                columns: new[] { "Id", "FeteId", "FeteTime", "ModifiedByUserId", "StudentId" },
                values: new object[] { 1, 1, new DateTime(2015, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Booking",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Room",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Fete",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Student",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
