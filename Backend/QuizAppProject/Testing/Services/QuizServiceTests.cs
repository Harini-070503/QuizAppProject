using Microsoft.EntityFrameworkCore;
using Moq;
using QuizAppProject.Context;
using QuizAppProject.Interfaces;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;
using QuizAppProject.Repositories;
using QuizAppProject.Services;

namespace Testing.Services
{
    public class QuizServiceTests : IDisposable
    {
        private readonly AppDbContext _ctx;
        private readonly QuizService _svc;

        private static readonly Guid CreatorId   = Guid.Parse("AAAAAAAA-0000-0000-0000-000000000001");
        private static readonly Guid EvaluatorId = Guid.Parse("AAAAAAAA-0000-0000-0000-000000000002");
        private static readonly Guid TakerId     = Guid.Parse("AAAAAAAA-0000-0000-0000-000000000003");
        private static readonly Guid CategoryId  = Guid.Parse("BBBBBBBB-0000-0000-0000-000000000001");
        private static readonly Guid QuizId1     = Guid.Parse("CCCCCCCC-0000-0000-0000-000000000001");
        private static readonly Guid QuizId2     = Guid.Parse("CCCCCCCC-0000-0000-0000-000000000002");
        private static readonly Guid GroupId1    = Guid.Parse("DDDDDDDD-0000-0000-0000-000000000001");

        public QuizServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _ctx = new AppDbContext(options);
            SeedData();

            var quizRepo     = new Repository<Guid, Quiz>(_ctx);
            var userRepo     = new Repository<Guid, User>(_ctx);
            var categoryRepo = new Repository<Guid, Category>(_ctx);
            var questionRepo = new Repository<Guid, Question>(_ctx);

            _svc = new QuizService(quizRepo, userRepo, categoryRepo, questionRepo, _ctx);
        }

        private void SeedData()
        {
            _ctx.Users.AddRange(
                new User { UserId = CreatorId,   Username = "creator1",   Email = "creator@test.com",   PasswordHash = "x", Salt = "s", Role = "Creator",   CreatedAt = DateTime.UtcNow },
                new User { UserId = EvaluatorId, Username = "evaluator1", Email = "eval@test.com",      PasswordHash = "x", Salt = "s", Role = "Evaluator", CreatedAt = DateTime.UtcNow },
                new User { UserId = TakerId,     Username = "taker1",     Email = "taker@test.com",     PasswordHash = "x", Salt = "s", Role = "Taker",     CreatedAt = DateTime.UtcNow }
            );

            _ctx.Categories.Add(new Category
            {
                CategoryId   = CategoryId,
                CategoryName = "Science",
                CreatedAt    = DateTime.UtcNow
            });

            _ctx.Quizzes.AddRange(
                new Quiz
                {
                    QuizId          = QuizId1,
                    UserId          = CreatorId,
                    CategoryId      = CategoryId,
                    QuizName        = "Science Quiz",
                    Description     = "A science quiz",
                    PassMark        = 5,
                    TotalQuestion   = 2,
                    DifficultyLevel = "Easy",
                    CreatedAt       = DateTime.UtcNow,
                    Questions       = new List<Question>
                    {
                        new Question
                        {
                            QuestionId   = Guid.NewGuid(),
                            QuizId       = QuizId1,
                            QuestionText = "What is H2O?",
                            Marks        = 1,
                            CreatedAt    = DateTime.UtcNow,
                            Options      = new Option
                            {
                                OptionId      = Guid.NewGuid(),
                                OptionA       = "Water",
                                OptionB       = "Oxygen",
                                CorrectOption = "A",
                                CreatedAt     = DateTime.UtcNow
                            }
                        }
                    }
                },
                new Quiz
                {
                    QuizId          = QuizId2,
                    UserId          = EvaluatorId,
                    CategoryId      = CategoryId,
                    QuizName        = "Evaluator Quiz",
                    PassMark        = 3,
                    TotalQuestion   = 1,
                    DifficultyLevel = "Medium",
                    GroupId         = GroupId1,
                    CreatedAt       = DateTime.UtcNow,
                    Questions       = new List<Question>()
                }
            );

            _ctx.QuizGroups.Add(new QuizGroup
            {
                GroupId     = GroupId1,
                GroupName   = "Group A",
                EvaluatorId = EvaluatorId,
                CreatedAt   = DateTime.UtcNow
            });

            _ctx.QuizGroupMembers.Add(new QuizGroupMember
            {
                Id      = Guid.NewGuid(),
                GroupId = GroupId1,
                UserId  = TakerId,
                AddedAt = DateTime.UtcNow
            });

            _ctx.SaveChanges();
        }

        public void Dispose() => _ctx.Dispose();

        // ── Get ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Get_ExistingQuiz_ReturnsDto()
        {
            var result = await _svc.Get(QuizId1);
            Assert.NotNull(result);
            Assert.Equal("Science Quiz", result!.QuizName);
            Assert.Equal(CategoryId, result.Category.CategoryId);
        }

        [Fact]
        public async Task Get_UnknownQuiz_ReturnsNull()
        {
            var result = await _svc.Get(Guid.NewGuid());
            Assert.Null(result);
        }

        // ── GetAll ────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_NoFilter_ReturnsAllCreatorQuizzes()
        {
            // Creator quizzes visible to everyone; evaluator quiz only to members
            var result = await _svc.GetAll(requestingUserId: CreatorId);
            Assert.Contains(result, q => q.QuizId == QuizId1);
        }

        [Fact]
        public async Task GetAll_EvaluatorQuiz_HiddenFromNonMember()
        {
            // CreatorId is not a member of GroupId1
            var result = await _svc.GetAll(requestingUserId: CreatorId);
            Assert.DoesNotContain(result, q => q.QuizId == QuizId2);
        }

        [Fact]
        public async Task GetAll_EvaluatorQuiz_VisibleToGroupMember()
        {
            // TakerId IS a member of GroupId1
            var result = await _svc.GetAll(requestingUserId: TakerId);
            Assert.Contains(result, q => q.QuizId == QuizId2);
        }

        [Fact]
        public async Task GetAll_EvaluatorQuiz_VisibleToEvaluatorOwner()
        {
            var result = await _svc.GetAll(requestingUserId: EvaluatorId);
            Assert.Contains(result, q => q.QuizId == QuizId2);
        }

        [Fact]
        public async Task GetAll_CategoryFilter_ReturnsOnlyMatchingCategory()
        {
            var result = await _svc.GetAll(categoryId: CategoryId, requestingUserId: CreatorId);
            Assert.All(result, q => Assert.Equal(CategoryId, q.Category.CategoryId));
        }

        // ── Add ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Add_ValidCreator_CreatesQuiz()
        {
            var dto = new QuizCreateDto
            {
                QuizName        = "New Quiz",
                Description     = "A new quiz",
                CategoryId      = CategoryId,
                PassMark        = 3,
                TotalQuestion   = 1,
                DifficultyLevel = "Easy"
            };

            var result = await _svc.Add(CreatorId, dto);

            Assert.NotNull(result);
            Assert.Equal("New Quiz", result.QuizName);
            Assert.Equal(CreatorId, result.CreatorId);
        }

        [Fact]
        public async Task Add_ValidEvaluator_CreatesQuiz()
        {
            var dto = new QuizCreateDto
            {
                QuizName        = "Eval Quiz",
                Description     = "An eval quiz",
                CategoryId      = CategoryId,
                PassMark        = 2,
                TotalQuestion   = 1,
                DifficultyLevel = "Hard"
            };

            var result = await _svc.Add(EvaluatorId, dto);

            Assert.NotNull(result);
            Assert.Equal(EvaluatorId, result.CreatorId);
        }

        [Fact]
        public async Task Add_TakerRole_ThrowsUnauthorized()
        {
            var dto = new QuizCreateDto
            {
                QuizName   = "Taker Quiz",
                CategoryId = CategoryId,
                PassMark   = 1,
                TotalQuestion = 1
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _svc.Add(TakerId, dto));
        }

        [Fact]
        public async Task Add_UnknownCreator_ThrowsKeyNotFound()
        {
            var dto = new QuizCreateDto
            {
                QuizName   = "Ghost Quiz",
                CategoryId = CategoryId,
                PassMark   = 1,
                TotalQuestion = 1
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.Add(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task Add_UnknownCategory_ThrowsKeyNotFound()
        {
            var dto = new QuizCreateDto
            {
                QuizName   = "Quiz",
                CategoryId = Guid.NewGuid(),
                PassMark   = 1,
                TotalQuestion = 1
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.Add(CreatorId, dto));
        }

        // ── Update ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Update_ValidOwner_UpdatesQuiz()
        {
            var dto = new QuizUpdateDto
            {
                QuizName        = "Updated Science Quiz",
                CategoryId      = CategoryId,
                PassMark        = 7,
                TotalQuestion   = 2,
                DifficultyLevel = "Hard"
            };

            var result = await _svc.Update(QuizId1, dto, CreatorId);

            Assert.Equal("Updated Science Quiz", result.QuizName);
            Assert.Equal(7, result.PassMark);
        }

        [Fact]
        public async Task Update_WrongOwner_ThrowsUnauthorized()
        {
            var dto = new QuizUpdateDto
            {
                QuizName      = "Hack",
                CategoryId    = CategoryId,
                PassMark      = 1,
                TotalQuestion = 1
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _svc.Update(QuizId1, dto, EvaluatorId));
        }

        [Fact]
        public async Task Update_UnknownQuiz_ThrowsKeyNotFound()
        {
            var dto = new QuizUpdateDto
            {
                QuizName      = "X",
                CategoryId    = CategoryId,
                PassMark      = 1,
                TotalQuestion = 1
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.Update(Guid.NewGuid(), dto, CreatorId));
        }

        // ── Delete ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_ValidOwner_DeletesQuiz()
        {
            var result = await _svc.Delete(QuizId1, CreatorId);
            Assert.True(result);
            Assert.Null(await _svc.Get(QuizId1));
        }

        [Fact]
        public async Task Delete_WrongOwner_ThrowsUnauthorized()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _svc.Delete(QuizId1, EvaluatorId));
        }

        [Fact]
        public async Task Delete_UnknownQuiz_ReturnsFalse()
        {
            var result = await _svc.Delete(Guid.NewGuid(), CreatorId);
            Assert.False(result);
        }
    }
}
