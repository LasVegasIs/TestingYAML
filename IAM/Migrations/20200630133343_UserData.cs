using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAM.Migrations
{
    public partial class UserData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NewsletterSubscribed",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserDatas",
                columns: table => new
                {
                    AccountId = table.Column<int>(nullable: false),
                    DateOfBirth = table.Column<DateTime>(nullable: true),
                    Gender = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDatas", x => x.AccountId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDatas");

            migrationBuilder.DropColumn(
                name: "NewsletterSubscribed",
                table: "AspNetUsers");
        }
    }
}
