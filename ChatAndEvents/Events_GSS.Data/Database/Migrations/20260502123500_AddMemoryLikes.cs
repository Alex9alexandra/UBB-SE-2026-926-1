using System;
using ChatAndEvents.Data.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatAndEvents.Data.Database.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260502123500_AddMemoryLikes")]
public partial class AddMemoryLikes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MemoryLikes",
            columns: table => new
            {
                MemoryId = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MemoryLikes", x => new { x.MemoryId, x.UserId });
                table.ForeignKey(
                    name: "FK_MemoryLikes_Memory",
                    column: x => x.MemoryId,
                    principalTable: "Memories",
                    principalColumn: "MemoryId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MemoryLikes_User",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "MemoryLikes");
    }
}
