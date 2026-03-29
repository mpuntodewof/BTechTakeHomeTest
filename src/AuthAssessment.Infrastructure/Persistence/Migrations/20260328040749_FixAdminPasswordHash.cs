using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthAssessment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixAdminPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "PasswordHash",
                value: "$2a$11$PmixqSgTA.5lktVendFxfO97LAAJ9dK11iAfgoeAavOPMUSHZAsi6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "PasswordHash",
                value: "$2a$11$K6GUL1Bk0Y7Z8K1Q5e8x5eFJXKJYFgQ5G8G5GZg3jH5ViKz5v5YW2");
        }
    }
}
