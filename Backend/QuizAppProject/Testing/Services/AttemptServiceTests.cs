using Microsoft.EntityFrameworkCore;
using QuizAppProject.Context;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;
using QuizAppProject.Repositories;
using QuizAppProject.Services;

namespace Testing.Services
{
    public class AttemptServiceTests : IDisposable
    {
        private readonly AppDbContext _ctx;
        private readonly AttemptService _svc;

        private static readonly Guid CreatorId   = Guid.Parse("10000000-0000-0000-0000-000000000001");
        private static readonly Guid EvalId      = Guid.Parse("10000000-0000-0000-0000-000000000002");
        private static readonly Guid TakerId     = Guid.Parse("10000000-0000-0000-0000-000000000003");
        private static readonly Guid CatId       = Guid.Parse("20000000-0000-0000-0000-000000000001");
        private static readonly Guid QuizId1     = Guid.Parse("30000000-0000-0000-0000-000000000001"); // Creator quiz
        private static readonly Guid QuizId2     = Guid.Parse("30000000-0000-0000-0000-000000000002"); // Evaluator quiz
        private static readonly Guid QuizId3     = Guid.Parse("30000000-0000-0000-0000-000000000003"); // Timed quiz
        private static readonly Guid Q1Id        = Guid.Parse("40000000-0000-0000-0000-000000000001");
        private static readonly Guid Q2Id        = Guid.Parse("40000000-0000-0000-0000-000000000002");
        private static readonly Guid Q3Id        = Guid.Parse("40000000-0000-0000-0000-000000000003");

        public AttemptServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _ctx = new AppDbContext(options);
            SeedData();

            var quizRepo    = new Repository<Guid, Quiz>(_ctx);
            var attemptRepo = new Repository<Guid, AttemptAnswer>(_ctx);
            _svc = new AttemptService(quizRepo, attemptRepo, _ctx);
        }

        private void SeedData()
        {
            _ctx.Users.AddRange(
                new User { UserId = CreatorId, Username = "creator", Email = "c@t.com", PasswordHash = "x", Salt = "s", Role = "Creator",   CreatedAt = DateTime.UtcNow },
                new User { UserId = EvalId,    Username = "eval",    Email = "e@t.com", PasswordHash = "x", Salt = "s", Role = "Evaluator", CreatedAt = DateTime.UtcNow },
                new User { UserId = TakerId,   Username = "taker",   Email = "t@t.com", PasswordHash = "x", Salt = "s", Role = "Taker",     CreatedAt = DateTime.UtcNow }
            );

            _ctx.Categories.Add(new Category { CategoryId = CatId, CategoryName = "Test", CreatedAt = DateTime.UtcNow });

            // Creator quiz — 2 questions, marks 1 each
            _ctx.Quizzes.Add(new Quiz
            {
                QuizId = QuizId1, UserId = CreatorId, CategoryId = CatId,
                QuizName = "Creator Quiz", Description = "d", PassMark = 1, TotalQuestion = 2,
                DifficultyLevel = "Easy", CreatedAt = DateTime.UtcNow,
                Questions = new List<Question>
                {
                    new Question { QuestionId = Q1Id, QuizId = QuizId1, QuestionText = "Q1", Marks = 1, CreatedAt = DateTime.UtcNow,
                        Options = new Option { OptionId = Guid.NewGuid(), OptionA = "A", OptionB = "B", CorrectOption = "A", CreatedAt = DateTime.UtcNow } },
                    new Question { QuestionId = Q2Id, QuizId = QuizId1, QuestionText = "Q2", Marks = 2, CreatedAt = DateTime.UtcNow,
                        Options = new Option { OptionId = Guid.NewGuid(), OptionA = "X", OptionB = "Y", CorrectOption = "B", CreatedAt = DateTime.UtcNow } }
                }
            });

            // Evaluator quiz — 1 question
            _ctx.Quizzes.Add(new Quiz
            {
                QuizId = QuizId2, UserId = EvalId, CategoryId = CatId,
                QuizName = "Eval Quiz", Description = "d", PassMark = 1, TotalQuestion = 1,
                DifficultyLevel = "Easy", CreatedAt = DateTime.UtcNow,
                Questions = new List<Question>
                {
                    new Question { QuestionId = Q3Id, QuizId = QuizId2, QuestionText = "Q3", Marks = 5, CreatedAt = DateTime.UtcNow,
                        Options = new Option { OptionId = Guid.NewGuid(), OptionA = "P", OptionB = "Q", CorrectOption = "A", CreatedAt = DateTime.UtcNow } }
                }
            });

            // Timed quiz — 1 minute limit
            _ctx.Quizzes.Add(new Quiz
            {
                QuizId = QuizId3, UserId = CreatorId, CategoryId = CatId,
                QuizName = "Timed Quiz", Description = "d", PassMark = 1, TotalQuestion = 1,
                DifficultyLevel = "Easy", TimeLimit = 1, CreatedAt = DateTime.UtcNow,
                Questions = new List<Question>
                {
                    new Question { QuestionId = Guid.NewGuid(), QuizId = QuizId3, QuestionText = "TQ1", Marks = 1, CreatedAt = DateTime.UtcNow,
                        Options = new Option { OptionId = Guid.NewGuid(), OptionA = "1", OptionB = "2", CorrectOption = "A", CreatedAt = DateTime.UtcNow } }
                }
            });

            _ctx.SaveChanges();
        }

        public void Dispose() => _ctx.Dispose();

        // ── SubmitAttempt — Creator quiz ──────────────────────────────────────

        [Fact]
        public async Task SubmitAttempt_AllCorrect_Returns100Percent()
        {
            var result = await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId  = QuizId1,
                UserId  = TakerId,
                Answers = new List<AttemptAnswerItemDto>
                {
                    new() { QuestionId = Q1Id, ChosenOption = "A" },
                    new() { QuestionId = Q2Id, ChosenOption = "B" }
                }
            });

            Assert.Equal(100.0, result.Percentage);
            Assert.Equal(3, result.TotalMark);   // 1+2
            Assert.False(result.IsPendingEvaluation);
        }

        [Fact]
        public async Task SubmitAttempt_AllWrong_Returns0Percent()
        {
            var result = await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId  = QuizId1,
                UserId  = TakerId,
                Answers = new List<AttemptAnswerItemDto>
                {
                    new() { QuestionId = Q1Id, ChosenOption = "B" },
                    new() { QuestionId = Q2Id, ChosenOption = "A" }
                }
            });

            Assert.Equal(0.0, result.Percentage);
            Assert.Equal(0, result.TotalMark);
        }

        [Fact]
        public async Task SubmitAttempt_PartialCorrect_CalculatesCorrectly()
        {
            // Q1 correct (1 mark), Q2 wrong (0 marks) → 1/3 = 33.33%
            var result = await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId  = QuizId1,
                UserId  = TakerId,
                Answers = new List<AttemptAnswerItemDto>
                {
                    new() { QuestionId = Q1Id, ChosenOption = "A" },
                    new() { QuestionId = Q2Id, ChosenOption = "A" }
                }
            });

            Assert.Equal(1, result.TotalMark);
            Assert.Equal(Math.Round(1.0 / 3.0 * 100, 2), result.Percentage);
        }

        [Fact]
        public async Task SubmitAttempt_FeedbackContainsCorrectOptions()
        {
            var result = await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId  = QuizId1,
                UserId  = TakerId,
                Answers = new List<AttemptAnswerItemDto>
                {
                    new() { QuestionId = Q1Id, ChosenOption = "B" },
                    new() { QuestionId = Q2Id, ChosenOption = "B" }
                }
            });

            var fb1 = result.Feedback.First(f => f.QuestionId == Q1Id);
            Assert.Equal("A",  fb1.CorrectOption);
            Assert.Equal("B",  fb1.YourOption);
            Assert.False(fb1.IsCorrect);

            var fb2 = result.Feedback.First(f => f.QuestionId == Q2Id);
            Assert.Equal("B",  fb2.CorrectOption);
            Assert.True(fb2.IsCorrect);
        }

        [Fact]
        public async Task SubmitAttempt_PersistsAttemptDetails()
        {
            var result = await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId  = QuizId1,
                UserId  = TakerId,
                Answers = new List<AttemptAnswerItemDto>
                {
                    new() { QuestionId = Q1Id, ChosenOption = "A" },
                    new() { QuestionId = Q2Id, ChosenOption = "B" }
                }
            });

            var details = await _ctx.AttemptAnswerDetails
                .Where(d => d.AttemptAnswerId == result.AttemptAnswerId)
                .ToListAsync();

            Assert.Equal(2, details.Count);
        }

        [Fact]
        public async Task SubmitAttempt_UnknownQuiz_ThrowsKeyNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
                {
                    QuizId  = Guid.NewGuid(),
                    UserId  = TakerId,
                    Answers = new List<AttemptAnswerItemDto>()
                }));
        }

        // ── SubmitAttempt — Evaluator quiz ────────────────────────────────────

        [Fact]
        public async Task SubmitAttempt_EvaluatorQuiz_IsPendingEvaluation()
        {
            var result = await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId  = QuizId2,
                UserId  = TakerId,
                Answers = new List<AttemptAnswerItemDto>
                {
                    new() { QuestionId = Q3Id, ChosenOption = "A" }
                }
            });

            Assert.True(result.IsPendingEvaluation);
            Assert.Equal(0, result.TotalMark);
            Assert.Equal(0.0, result.Percentage);
        }

        [Fact]
        public async Task SubmitAttempt_EvaluatorQuiz_SendsNotificationToEvaluator()
        {
            await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId  = QuizId2,
                UserId  = TakerId,
                Answers = new List<AttemptAnswerItemDto>
                {
                    new() { QuestionId = Q3Id, ChosenOption = "A" }
                }
            });

            var notif = await _ctx.Notifications
                .FirstOrDefaultAsync(n => n.UserId == EvalId && n.Type == "test_submitted");
            Assert.NotNull(notif);
            Assert.Contains("taker", notif!.Message);
        }

        // ── SubmitAttempt — Time limit ────────────────────────────────────────

        [Fact]
        public async Task SubmitAttempt_WithinTimeLimit_Succeeds()
        {
            var start = DateTime.UtcNow.AddSeconds(-30);
            var end   = DateTime.UtcNow;

            var result = await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId       = QuizId3,
                UserId       = TakerId,
                StartedAtUtc = start,
                EndedAtUtc   = end,
                Answers      = new List<AttemptAnswerItemDto>()
            });

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SubmitAttempt_ExceedsTimeLimit_ThrowsInvalidOperation()
        {
            var start = DateTime.UtcNow.AddMinutes(-5); // 5 min elapsed, limit is 1 min
            var end   = DateTime.UtcNow;

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
                {
                    QuizId       = QuizId3,
                    UserId       = TakerId,
                    StartedAtUtc = start,
                    EndedAtUtc   = end,
                    Answers      = new List<AttemptAnswerItemDto>()
                }));
        }

        // ── GetAttemptsByUser ─────────────────────────────────────────────────

        [Fact]
        public async Task GetAttemptsByUser_ReturnsUserAttempts()
        {
            await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId = QuizId1, UserId = TakerId,
                Answers = new List<AttemptAnswerItemDto>
                {
                    new() { QuestionId = Q1Id, ChosenOption = "A" },
                    new() { QuestionId = Q2Id, ChosenOption = "B" }
                }
            });

            var result = await _svc.GetAttemptsByUser(TakerId);

            Assert.Single(result);
            Assert.Equal(QuizId1, result[0].QuizId);
        }

        [Fact]
        public async Task GetAttemptsByUser_UnknownUser_ReturnsEmpty()
        {
            var result = await _svc.GetAttemptsByUser(Guid.NewGuid());
            Assert.Empty(result);
        }

        // ── GetAttempt ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAttempt_ExistingAttempt_ReturnsFeedback()
        {
            var submitted = await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId = QuizId1, UserId = TakerId,
                Answers = new List<AttemptAnswerItemDto>
                {
                    new() { QuestionId = Q1Id, ChosenOption = "A" },
                    new() { QuestionId = Q2Id, ChosenOption = "B" }
                }
            });

            var result = await _svc.GetAttempt(submitted.AttemptAnswerId);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Feedback.Count);
        }

        [Fact]
        public async Task GetAttempt_UnknownAttempt_ReturnsNull()
        {
            var result = await _svc.GetAttempt(Guid.NewGuid());
            Assert.Null(result);
        }

        // ── UpdateScore ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateScore_ValidAttempt_UpdatesMarksAndPercentage()
        {
            var submitted = await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId = QuizId2, UserId = TakerId,
                Answers = new List<AttemptAnswerItemDto>
                {
                    new() { QuestionId = Q3Id, ChosenOption = "A" }
                }
            });

            var ok = await _svc.UpdateScore(submitted.AttemptAnswerId, new UpdateScoreDto
            {
                NewTotalMark   = 4,
                QuestionScores = new List<QuestionScoreDto>
                {
                    new() { QuestionId = Q3Id, MarksAwarded = 4 }
                }
            });

            Assert.True(ok);

            var updated = await _ctx.AttemptAnswers.FindAsync(submitted.AttemptAnswerId);
            Assert.Equal(4, updated!.TotalMark);
            Assert.Equal(80.0, updated.Percentage); // 4/5 = 80%
        }

        [Fact]
        public async Task UpdateScore_SendsNotificationToStudent()
        {
            var submitted = await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId = QuizId2, UserId = TakerId,
                Answers = new List<AttemptAnswerItemDto>
                {
                    new() { QuestionId = Q3Id, ChosenOption = "A" }
                }
            });

            await _svc.UpdateScore(submitted.AttemptAnswerId, new UpdateScoreDto
            {
                NewTotalMark   = 5,
                QuestionScores = new List<QuestionScoreDto>
                {
                    new() { QuestionId = Q3Id, MarksAwarded = 5 }
                }
            });

            var notif = await _ctx.Notifications
                .FirstOrDefaultAsync(n => n.UserId == TakerId && n.Type == "score_updated");
            Assert.NotNull(notif);
        }

        [Fact]
        public async Task UpdateScore_UnknownAttempt_ReturnsFalse()
        {
            var ok = await _svc.UpdateScore(Guid.NewGuid(), new UpdateScoreDto
            {
                NewTotalMark   = 5,
                QuestionScores = new List<QuestionScoreDto>()
            });

            Assert.False(ok);
        }

        // ── GetSubmissionsByQuiz ──────────────────────────────────────────────

        [Fact]
        public async Task GetSubmissionsByQuiz_ReturnsAllSubmissions()
        {
            await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId = QuizId1, UserId = TakerId,
                Answers = new List<AttemptAnswerItemDto>
                {
                    new() { QuestionId = Q1Id, ChosenOption = "A" },
                    new() { QuestionId = Q2Id, ChosenOption = "B" }
                }
            });

            var result = await _svc.GetSubmissionsByQuiz(QuizId1);

            Assert.Single(result);
            Assert.Equal(TakerId, result[0].UserId);
            Assert.Equal("Creator Quiz", result[0].QuizName);
        }

        [Fact]
        public async Task GetSubmissionsByQuiz_NoSubmissions_ReturnsEmpty()
        {
            var result = await _svc.GetSubmissionsByQuiz(QuizId1);
            Assert.Empty(result);
        }

        // ── GetSubmissionDetail ───────────────────────────────────────────────

        [Fact]
        public async Task GetSubmissionDetail_ReturnsAnswersWithQuestionText()
        {
            var submitted = await _svc.SubmitAttempt(TakerId, new AttemptSubmitDto
            {
                QuizId = QuizId1, UserId = TakerId,
                Answers = new List<AttemptAnswerItemDto>
                {
                    new() { QuestionId = Q1Id, ChosenOption = "A" },
                    new() { QuestionId = Q2Id, ChosenOption = "B" }
                }
            });

            var detail = await _svc.GetSubmissionDetail(submitted.AttemptAnswerId);

            Assert.NotNull(detail);
            Assert.Equal(2, detail!.Answers.Count);
            Assert.All(detail.Answers, a => Assert.False(string.IsNullOrEmpty(a.QuestionText)));
        }

        [Fact]
        public async Task GetSubmissionDetail_UnknownAttempt_ReturnsNull()
        {
            var result = await _svc.GetSubmissionDetail(Guid.NewGuid());
            Assert.Null(result);
        }
    }
}
