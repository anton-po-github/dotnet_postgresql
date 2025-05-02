using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_postgresql.Migrations.Users
{
    /// <inheritdoc />
    public partial class AddOwnerIdToBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Users");
        }
    }
}
