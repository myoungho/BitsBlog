using System;
using BitsBlog.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BitsBlog.Infrastructure.Migrations
{
    [DbContext(typeof(BitsBlogDbContext))]
    [Migration("20250904120000_AddAuthorFields")]
    public partial class AddAuthorFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorLoginId",
                table: "Posts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorDisplayName",
                table: "Posts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorLoginId",
                table: "Comments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorDisplayName",
                table: "Comments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AuthorLoginId", table: "Posts");
            migrationBuilder.DropColumn(name: "AuthorDisplayName", table: "Posts");
            migrationBuilder.DropColumn(name: "AuthorLoginId", table: "Comments");
            migrationBuilder.DropColumn(name: "AuthorDisplayName", table: "Comments");
        }
    }
}
