using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizAppProject.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlySubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizPayments_Quizzes_QuizId",
                table: "QuizPayments");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuizId",
                table: "QuizPayments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "QuizPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionType",
                table: "QuizPayments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizPayments_Quizzes_QuizId",
                table: "QuizPayments",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "QuizId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizPayments_Quizzes_QuizId",
                table: "QuizPayments");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "QuizPayments");

            migrationBuilder.DropColumn(
                name: "SubscriptionType",
                table: "QuizPayments");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuizId",
                table: "QuizPayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizPayments_Quizzes_QuizId",
                table: "QuizPayments",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "QuizId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
