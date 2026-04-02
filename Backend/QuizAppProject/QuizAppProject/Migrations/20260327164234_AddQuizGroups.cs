using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizAppProject.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "Quizzes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuizGroups",
                columns: table => new
                {
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EvaluatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizGroups", x => x.GroupId);
                    table.ForeignKey(
                        name: "FK_QuizGroups_Users_EvaluatorId",
                        column: x => x.EvaluatorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizGroupMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizGroupMembers_QuizGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "QuizGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizGroupMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_GroupId",
                table: "Quizzes",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizGroupMembers_GroupId_UserId",
                table: "QuizGroupMembers",
                columns: new[] { "GroupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizGroupMembers_UserId",
                table: "QuizGroupMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizGroups_EvaluatorId",
                table: "QuizGroups",
                column: "EvaluatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_QuizGroups_GroupId",
                table: "Quizzes",
                column: "GroupId",
                principalTable: "QuizGroups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_QuizGroups_GroupId",
                table: "Quizzes");

            migrationBuilder.DropTable(
                name: "QuizGroupMembers");

            migrationBuilder.DropTable(
                name: "QuizGroups");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_GroupId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Quizzes");
        }
    }
}
