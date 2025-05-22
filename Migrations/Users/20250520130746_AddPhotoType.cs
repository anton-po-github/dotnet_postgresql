using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_postgresql.Migrations.Users
{
    /// <inheritdoc />
    public partial class AddPhotoType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoType",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoType",
                table: "Users");
        }
    }
}
