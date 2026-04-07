using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizAppProject.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizAllocationAndPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizAllocations",
                columns: table => new
                {
                    AllocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    QuizId       = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId       = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllocatedAt  = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAllocations", x => x.AllocationId);
                    table.ForeignKey(
                        name: "FK_QuizAllocations_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "QuizId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAllocations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAllocations_UserId",
                table: "QuizAllocations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAllocations_QuizId_UserId",
                table: "QuizAllocations",
                columns: new[] { "QuizId", "UserId" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "QuizPayments",
                columns: table => new
                {
                    PaymentId      = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    UserId         = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizId         = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount         = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status         = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransactionRef = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsUsed         = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt      = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    PaidAt         = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizPayments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_QuizPayments_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "QuizId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizPayments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizPayments_QuizId",
                table: "QuizPayments",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizPayments_UserId",
                table: "QuizPayments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "QuizPayments");
            migrationBuilder.DropTable(name: "QuizAllocations");
        }
    }
}
