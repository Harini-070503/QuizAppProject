using Microsoft.EntityFrameworkCore;
using QuizAppProject.Context;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;
using QuizAppProject.Repositories;
using QuizAppProject.Services;

namespace Testing.Services
{
    public class QuestionServiceTests : IDisposable
    {
        private readonly AppDbContext _ctx;
        private readonly QuestionService _svc;

        private static readonly Guid QuizId1      = Guid.Parse("EEEEEEEE-0000-0000-0000-000000000001");
        private static readonly Guid QuizId2      = Guid.Parse("EEEEEEEE-0000-0000-0000-000000000002");
        private static readonly Guid QuestionId1  = Guid.Parse("FFFFFFFF-0000-0000-0000-000000000001");
        private static readonly Guid QuestionId2  = Guid.Parse("FFFFFFFF-0000-0000-0000-000000000002");
        private static readonly Guid CreatorId    = Guid.Parse("AAAAAAAA-0000-0000-0000-000000000099");
        private static readonly Guid CategoryId   = Guid.Parse("BBBBBBBB-0000-0000-0000-000000000099");

        public QuestionServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _ctx = new AppDbContext(options);
            SeedData();

            var questionRepo = new Repository<Guid, Question>(_ctx);
            var quizRepo     = new Repository<Guid, Quiz>(_ctx);
            _svc = new QuestionService(questionRepo, quizRepo);
        }

        private void SeedData()
        {
            _ctx.Users.Add(new User
            {
                UserId = CreatorId, Username = "creator", Email = "c@test.com",
                PasswordHash = "x", Salt = "s", Role = "Creator", CreatedAt = DateTime.UtcNow
            });

            _ctx.Categories.Add(new Category
            {
                CategoryId = CategoryId, CategoryName = "Math", CreatedAt = DateTime.UtcNow
            });

            _ctx.Quizzes.AddRange(
                new Quiz
                {
                    QuizId = QuizId1, UserId = CreatorId, CategoryId = CategoryId,
                    QuizName = "Math Quiz", Description = "desc",
                    PassMark = 5, TotalQuestion = 2, DifficultyLevel = "Easy",
                    CreatedAt = DateTime.UtcNow
                },
                new Quiz
                {
                    QuizId = QuizId2, UserId = CreatorId, CategoryId = CategoryId,
                    QuizName = "Empty Quiz", Description = "desc",
                    PassMark = 3, TotalQuestion = 0, DifficultyLevel = "Hard",
                    CreatedAt = DateTime.UtcNow
                }
            );

            _ctx.Questions.AddRange(
                new Question
                {
                    QuestionId   = QuestionId1,
                    QuizId       = QuizId1,
                    QuestionText = "What is 2+2?",
                    Marks        = 2,
                    CreatedAt    = DateTime.UtcNow,
                    Options      = new Option
                    {
                        OptionId      = Guid.NewGuid(),
                        OptionA       = "3",
                        OptionB       = "4",
                        OptionC       = "5",
                        OptionD       = "6",
                        CorrectOption = "B",
                        CreatedAt     = DateTime.UtcNow
                    }
                },
                new Question
                {
                    QuestionId   = QuestionId2,
                    QuizId       = QuizId1,
                    QuestionText = "What is 3x3?",
                    Marks        = 3,
                    CreatedAt    = DateTime.UtcNow,
                    Options      = new Option
                    {
                        OptionId      = Guid.NewGuid(),
                        OptionA       = "6",
                        OptionB       = "9",
                        OptionC       = "12",
                        OptionD       = "15",
                        CorrectOption = "B",
                        CreatedAt     = DateTime.UtcNow
                    }
                }
            );

            _ctx.SaveChanges();
        }

        public void Dispose() => _ctx.Dispose();

        // ── Get ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Get_ExistingQuestion_ReturnsDto()
        {
            var result = await _svc.Get(QuestionId1);

            Assert.NotNull(result);
            Assert.Equal("What is 2+2?", result!.QuestionText);
            Assert.Equal(2, result.Marks);
            Assert.Equal("B", result.Options?.CorrectOption);
        }

        [Fact]
        public async Task Get_UnknownQuestion_ReturnsNull()
        {
            var result = await _svc.Get(Guid.NewGuid());
            Assert.Null(result);
        }

        // ── GetByQuiz ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByQuiz_QuizWithQuestions_ReturnsAll()
        {
            var result = await _svc.GetByQuiz(QuizId1);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, q => q.QuestionText == "What is 2+2?");
            Assert.Contains(result, q => q.QuestionText == "What is 3x3?");
        }

        [Fact]
        public async Task GetByQuiz_EmptyQuiz_ReturnsEmptyList()
        {
            var result = await _svc.GetByQuiz(QuizId2);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByQuiz_UnknownQuiz_ReturnsEmptyList()
        {
            var result = await _svc.GetByQuiz(Guid.NewGuid());
            Assert.Empty(result);
        }

        // ── Add ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Add_ValidRequest_CreatesQuestion()
        {
            var dto = new QuestionCreateDto
            {
                QuestionText = "What is 10/2?",
                Marks        = 1,
                Options      = new OptionCreateDto
                {
                    OptionA       = "3",
                    OptionB       = "5",
                    OptionC       = "7",
                    OptionD       = "9",
                    CorrectOption = "B"
                }
            };

            var result = await _svc.Add(QuizId1, dto);

            Assert.NotNull(result);
            Assert.Equal("What is 10/2?", result.QuestionText);
            Assert.Equal(1, result.Marks);
            Assert.Equal("B", result.Options.CorrectOption);
        }

        [Fact]
        public async Task Add_ZeroMarks_DefaultsToOne()
        {
            var dto = new QuestionCreateDto
            {
                QuestionText = "Zero marks question",
                Marks        = 0,
                Options      = new OptionCreateDto
                {
                    OptionA = "A", OptionB = "B", CorrectOption = "A"
                }
            };

            var result = await _svc.Add(QuizId1, dto);

            Assert.Equal(1, result.Marks);
        }

        [Fact]
        public async Task Add_UnknownQuiz_ThrowsKeyNotFoundException()
        {
            var dto = new QuestionCreateDto
            {
                QuestionText = "Ghost question",
                Marks        = 1,
                Options      = new OptionCreateDto
                {
                    OptionA = "A", OptionB = "B", CorrectOption = "A"
                }
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.Add(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task Add_WithTwoOptions_StoresCorrectly()
        {
            var dto = new QuestionCreateDto
            {
                QuestionText = "True or False?",
                Marks        = 1,
                Options      = new OptionCreateDto
                {
                    OptionA       = "True",
                    OptionB       = "False",
                    CorrectOption = "A"
                }
            };

            var result = await _svc.Add(QuizId1, dto);

            Assert.Equal("True",  result.Options.OptionA);
            Assert.Equal("False", result.Options.OptionB);
            Assert.Null(result.Options.OptionC);
            Assert.Null(result.Options.OptionD);
        }

        // ── Update ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Update_ValidRequest_UpdatesQuestion()
        {
            var dto = new QuestionCreateDto
            {
                QuestionText = "What is 2+2? (updated)",
                Marks        = 5,
                Options      = new OptionCreateDto
                {
                    OptionA       = "3",
                    OptionB       = "4",
                    OptionC       = "5",
                    OptionD       = "6",
                    CorrectOption = "B"
                }
            };

            var result = await _svc.Update(QuestionId1, dto);

            Assert.Equal("What is 2+2? (updated)", result.QuestionText);
            Assert.Equal(5, result.Marks);
        }

        [Fact]
        public async Task Update_ZeroMarks_DefaultsToOne()
        {
            var dto = new QuestionCreateDto
            {
                QuestionText = "Updated",
                Marks        = 0,
                Options      = new OptionCreateDto
                {
                    OptionA = "A", OptionB = "B", CorrectOption = "A"
                }
            };

            var result = await _svc.Update(QuestionId1, dto);

            Assert.Equal(1, result.Marks);
        }

        [Fact]
        public async Task Update_UnknownQuestion_ThrowsKeyNotFoundException()
        {
            var dto = new QuestionCreateDto
            {
                QuestionText = "Ghost",
                Marks        = 1,
                Options      = new OptionCreateDto
                {
                    OptionA = "A", OptionB = "B", CorrectOption = "A"
                }
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.Update(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task Update_ChangesCorrectOption()
        {
            var dto = new QuestionCreateDto
            {
                QuestionText = "What is 2+2?",
                Marks        = 2,
                Options      = new OptionCreateDto
                {
                    OptionA       = "3",
                    OptionB       = "4",
                    OptionC       = "5",
                    OptionD       = "6",
                    CorrectOption = "C"   // changed from B to C
                }
            };

            var result = await _svc.Update(QuestionId1, dto);

            Assert.Equal("C", result.Options.CorrectOption);
        }

        // ── Delete ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_ExistingQuestion_ReturnsTrue()
        {
            var result = await _svc.Delete(QuestionId1);

            Assert.True(result);
            Assert.Null(await _svc.Get(QuestionId1));
        }

        [Fact]
        public async Task Delete_UnknownQuestion_ReturnsFalse()
        {
            var result = await _svc.Delete(Guid.NewGuid());
            Assert.False(result);
        }

        [Fact]
        public async Task Delete_RemovesOnlyTargetQuestion()
        {
            await _svc.Delete(QuestionId1);

            var remaining = await _svc.GetByQuiz(QuizId1);
            Assert.Single(remaining);
            Assert.Equal(QuestionId2, remaining[0].QuestionId);
        }
    }
}
