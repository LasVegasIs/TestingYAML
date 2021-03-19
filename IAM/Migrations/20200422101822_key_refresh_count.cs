using Microsoft.EntityFrameworkCore.Migrations;

namespace IAM.Migrations
{
    public partial class key_refresh_count : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RefreshCount",
                table: "SessionTokens",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshCount",
                table: "SessionTokens");
        }
    }
}
