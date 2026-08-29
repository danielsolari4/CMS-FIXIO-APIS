using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ray.Model.Migrations
{
    public partial class FixMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageContent_AssetContent",
                table: "PageContent");

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

            migrationBuilder.AddForeignKey(
                name: "FK_PageContent_AssetContent_Id",
                table: "PageContent",
                column: "Id",
                principalTable: "AssetContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetContent_PageContent_PageContentId",
                table: "AssetContent");

            migrationBuilder.DropForeignKey(
                name: "FK_PageContent_AssetContent_Id",
                table: "PageContent");

            migrationBuilder.DropIndex(
                name: "IX_AssetContent_PageContentId",
                table: "AssetContent");

            migrationBuilder.DropColumn(
                name: "PageContentId",
                table: "AssetContent");

        

            migrationBuilder.AddForeignKey(
                name: "FK_PageContent_AssetContent",
                table: "PageContent",
                column: "Id",
                principalTable: "AssetContent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
