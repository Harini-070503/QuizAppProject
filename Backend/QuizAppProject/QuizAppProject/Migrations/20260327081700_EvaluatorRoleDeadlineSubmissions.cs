using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizAppProject.Migrations
{
    /// <inheritdoc />
    public partial class EvaluatorRoleDeadlineSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "Quizzes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttemptAnswerDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    AttemptAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChosenOption = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    MarksAwarded = table.Column<int>(type: "int", nullable: false),
                    MaxMarks = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttemptAnswerDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttemptAnswerDetails_AttemptAnswers_AttemptAnswerId",
                        column: x => x.AttemptAnswerId,
                        principalTable: "AttemptAnswers",
                        principalColumn: "AttemptAnswerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttemptAnswerDetails_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "QuestionId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttemptAnswerDetails_AttemptAnswerId",
                table: "AttemptAnswerDetails",
                column: "AttemptAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_AttemptAnswerDetails_QuestionId",
                table: "AttemptAnswerDetails",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttemptAnswerDetails");

            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "Quizzes");
        }
    }
}
