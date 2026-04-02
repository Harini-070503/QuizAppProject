using Microsoft.EntityFrameworkCore;
using QuizAppProject.Context;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;
using QuizAppProject.Repositories;
using QuizAppProject.Services;

namespace Testing.Services
{
    public class OptionServiceTests : IDisposable
    {
        private readonly AppDbContext _ctx;
        private readonly OptionService _svc;

        private static readonly Guid QuizId      = Guid.Parse("11111111-AAAA-0000-0000-000000000001");
        private static readonly Guid QuestionId1 = Guid.Parse("22222222-AAAA-0000-0000-000000000001"); // has options
        private static readonly Guid QuestionId2 = Guid.Parse("22222222-AAAA-0000-0000-000000000002"); // no options
        private static readonly Guid OptionId1   = Guid.Parse("33333333-AAAA-0000-0000-000000000001");
        private static readonly Guid CreatorId   = Guid.Parse("44444444-AAAA-0000-0000-000000000001");
        private static readonly Guid CategoryId  = Guid.Parse("55555555-AAAA-0000-0000-000000000001");

        public OptionServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _ctx = new AppDbContext(options);
            SeedData();

            var optionRepo   = new Repository<Guid, Option>(_ctx);
            var questionRepo = new Repository<Guid, Question>(_ctx);
            _svc = new OptionService(optionRepo, questionRepo);
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
                CategoryId = CategoryId, CategoryName = "General", CreatedAt = DateTime.UtcNow
            });

            _ctx.Quizzes.Add(new Quiz
            {
                QuizId = QuizId, UserId = CreatorId, CategoryId = CategoryId,
                QuizName = "Test Quiz", Description = "desc",
                PassMark = 5, TotalQuestion = 2, DifficultyLevel = "Easy",
                CreatedAt = DateTime.UtcNow
            });

            // QuestionId1 — already has options
            _ctx.Questions.Add(new Question
            {
                QuestionId   = QuestionId1,
                QuizId       = QuizId,
                QuestionText = "What is 1+1?",
                Marks        = 1,
                CreatedAt    = DateTime.UtcNow,
                Options      = new Option
                {
                    OptionId      = OptionId1,
                    OptionA       = "1",
                    OptionB       = "2",
                    OptionC       = "3",
                    OptionD       = "4",
                    CorrectOption = "B",
                    CreatedAt     = DateTime.UtcNow
                }
            });

            // QuestionId2 — no options yet
            _ctx.Questions.Add(new Question
            {
                QuestionId   = QuestionId2,
                QuizId       = QuizId,
                QuestionText = "What is 2+2?",
                Marks        = 1,
                CreatedAt    = DateTime.UtcNow
            });

            _ctx.SaveChanges();
        }

        public void Dispose() => _ctx.Dispose();

        // ── Get ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Get_ExistingOption_ReturnsDto()
        {
            var result = await _svc.Get(OptionId1);

            Assert.NotNull(result);
            Assert.Equal("1",  result!.OptionA);
            Assert.Equal("2",  result.OptionB);
            Assert.Equal("3",  result.OptionC);
            Assert.Equal("4",  result.OptionD);
            Assert.Equal("B",  result.CorrectOption);
        }

        [Fact]
        public async Task Get_UnknownOption_ReturnsNull()
        {
            var result = await _svc.Get(Guid.NewGuid());
            Assert.Null(result);
        }

        // ── Add ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Add_QuestionWithNoOptions_CreatesOption()
        {
            var dto = new OptionCreateDto
            {
                OptionA       = "Yes",
                OptionB       = "No",
                CorrectOption = "A"
            };

            var result = await _svc.Add(QuestionId2, dto);

            Assert.NotNull(result);
            Assert.Equal("Yes", result.OptionA);
            Assert.Equal("No",  result.OptionB);
            Assert.Equal("A",   result.CorrectOption);
        }

        [Fact]
        public async Task Add_QuestionAlreadyHasOptions_ThrowsInvalidOperation()
        {
            var dto = new OptionCreateDto
            {
                OptionA = "X", OptionB = "Y", CorrectOption = "A"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _svc.Add(QuestionId1, dto));
        }

        [Fact]
        public async Task Add_UnknownQuestion_ThrowsKeyNotFoundException()
        {
            var dto = new OptionCreateDto
            {
                OptionA = "X", OptionB = "Y", CorrectOption = "A"
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.Add(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task Add_WithFourOptions_StoresAllOptions()
        {
            var dto = new OptionCreateDto
            {
                OptionA       = "Alpha",
                OptionB       = "Beta",
                OptionC       = "Gamma",
                OptionD       = "Delta",
                CorrectOption = "C"
            };

            var result = await _svc.Add(QuestionId2, dto);

            Assert.Equal("Alpha", result.OptionA);
            Assert.Equal("Beta",  result.OptionB);
            Assert.Equal("Gamma", result.OptionC);
            Assert.Equal("Delta", result.OptionD);
            Assert.Equal("C",     result.CorrectOption);
        }

        [Fact]
        public async Task Add_WithTwoOptions_NullCAndD()
        {
            var dto = new OptionCreateDto
            {
                OptionA       = "True",
                OptionB       = "False",
                CorrectOption = "A"
            };

            var result = await _svc.Add(QuestionId2, dto);

            Assert.Null(result.OptionC);
            Assert.Null(result.OptionD);
        }

        // ── Update ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Update_ExistingOption_UpdatesAllFields()
        {
            var dto = new OptionCreateDto
            {
                OptionA       = "10",
                OptionB       = "20",
                OptionC       = "30",
                OptionD       = "40",
                CorrectOption = "D"
            };

            var result = await _svc.Update(OptionId1, dto);

            Assert.Equal("10", result.OptionA);
            Assert.Equal("20", result.OptionB);
            Assert.Equal("30", result.OptionC);
            Assert.Equal("40", result.OptionD);
            Assert.Equal("D",  result.CorrectOption);
        }

        [Fact]
        public async Task Update_UnknownOption_ThrowsKeyNotFoundException()
        {
            var dto = new OptionCreateDto
            {
                OptionA = "X", OptionB = "Y", CorrectOption = "A"
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.Update(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task Update_ChangesCorrectOption()
        {
            var dto = new OptionCreateDto
            {
                OptionA       = "1",
                OptionB       = "2",
                OptionC       = "3",
                OptionD       = "4",
                CorrectOption = "A"   // was B, now A
            };

            var result = await _svc.Update(OptionId1, dto);

            Assert.Equal("A", result.CorrectOption);
        }

        [Fact]
        public async Task Update_ClearsOptionalOptions()
        {
            var dto = new OptionCreateDto
            {
                OptionA       = "Yes",
                OptionB       = "No",
                OptionC       = null,
                OptionD       = null,
                CorrectOption = "B"
            };

            var result = await _svc.Update(OptionId1, dto);

            Assert.Null(result.OptionC);
            Assert.Null(result.OptionD);
        }

        // ── Delete ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_ExistingOption_ReturnsTrue()
        {
            var result = await _svc.Delete(OptionId1);

            Assert.True(result);
            Assert.Null(await _svc.Get(OptionId1));
        }

        [Fact]
        public async Task Delete_UnknownOption_ReturnsFalse()
        {
            var result = await _svc.Delete(Guid.NewGuid());
            Assert.False(result);
        }
    }
}
