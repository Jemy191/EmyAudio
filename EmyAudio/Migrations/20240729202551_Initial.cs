using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmyAudio.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AudioInfos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    LastPlayed = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    TimePlayed = table.Column<uint>(type: "INTEGER", nullable: false),
                    Next = table.Column<string>(type: "TEXT", nullable: true),
                    Previous = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Downloaded = table.Column<bool>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Parent = table.Column<string>(type: "TEXT", nullable: true),
                    Next = table.Column<string>(type: "TEXT", nullable: true),
                    Previous = table.Column<string>(type: "TEXT", nullable: true),
                    Hidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    OriginalPlaylistId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "AudioSkips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Start = table.Column<long>(type: "INTEGER", nullable: false),
                    End = table.Column<long>(type: "INTEGER", nullable: false),
                    AudioInfoId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioSkips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AudioSkips_AudioInfos_AudioInfoId",
                        column: x => x.AudioInfoId,
                        principalTable: "AudioInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AudioInfoToTagJoin",
                columns: table => new
                {
                    AudioInfoId = table.Column<string>(type: "TEXT", nullable: false),
                    TagName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioInfoToTagJoin", x => new { x.AudioInfoId, x.TagName });
                    table.ForeignKey(
                        name: "FK_AudioInfoToTagJoin_AudioInfos_AudioInfoId",
                        column: x => x.AudioInfoId,
                        principalTable: "AudioInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AudioInfoToTagJoin_Tags_TagName",
                        column: x => x.TagName,
                        principalTable: "Tags",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AudioInfoToTagJoin_TagName",
                table: "AudioInfoToTagJoin",
                column: "TagName");

            migrationBuilder.CreateIndex(
                name: "IX_AudioSkips_AudioInfoId",
                table: "AudioSkips",
                column: "AudioInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AudioInfoToTagJoin");

            migrationBuilder.DropTable(
                name: "AudioSkips");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "AudioInfos");
        }
    }
}
