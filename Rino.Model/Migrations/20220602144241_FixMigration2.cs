using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rino.Model.Migrations
{
    public partial class FixMigration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetContent_PageContent_PageContentId",
                table: "AssetContent");
            
            migrationBuilder.DropIndex(
                name: "IX_AssetContent_PageContentId",
                table: "AssetContent");

            migrationBuilder.DropColumn(
                name: "PageContentId",
                table: "AssetContent");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PageContentId",
                table: "AssetContent",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetContent_PageContentId",
                table: "AssetContent",
                column: "PageContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetContent_PageContent_PageContentId",
                table: "AssetContent",
                column: "PageContentId",
                principalTable: "PageContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
