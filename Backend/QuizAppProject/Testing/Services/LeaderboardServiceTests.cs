using Microsoft.EntityFrameworkCore;
using QuizAppProject.Context;
using QuizAppProject.Models;
using QuizAppProject.Repositories;
using QuizAppProject.Services;

namespace Testing.Services
{
    public class LeaderboardServiceTests : IDisposable
    {
        private readonly AppDbContext _ctx;
        private readonly LeaderboardService _svc;

        private static readonly Guid UserId1    = Guid.Parse("AAAA0001-0000-0000-0000-000000000001");
        private static readonly Guid UserId2    = Guid.Parse("AAAA0001-0000-0000-0000-000000000002");
        private static readonly Guid UserId3    = Guid.Parse("AAAA0001-0000-0000-0000-000000000003");
        private static readonly Guid CatId1     = Guid.Parse("BBBB0001-0000-0000-0000-000000000001");
        private static readonly Guid CatId2     = Guid.Parse("BBBB0001-0000-0000-0000-000000000002");
        private static readonly Guid QuizId1    = Guid.Parse("CCCC0001-0000-0000-0000-000000000001");
        private static readonly Guid QuizId2    = Guid.Parse("CCCC0001-0000-0000-0000-000000000002");

        public LeaderboardServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _ctx = new AppDbContext(options);
            SeedData();

            var attemptRepo = new Repository<Guid, AttemptAnswer>(_ctx);
            _svc = new LeaderboardService(attemptRepo);
        }

        private void SeedData()
        {
            _ctx.Users.AddRange(
                new User { UserId = UserId1, Username = "alice", Email = "a@t.com", PasswordHash = "x", Salt = "s", Role = "Taker", CreatedAt = DateTime.UtcNow,
                           UserDetails = new UserDetails { Name = "Alice Smith" } },
                new User { UserId = UserId2, Username = "bob",   Email = "b@t.com", PasswordHash = "x", Salt = "s", Role = "Taker", CreatedAt = DateTime.UtcNow,
                           UserDetails = new UserDetails { Name = "Bob Jones" } },
                new User { UserId = UserId3, Username = "carol", Email = "c@t.com", PasswordHash = "x", Salt = "s", Role = "Taker", CreatedAt = DateTime.UtcNow }
            );

            _ctx.Categories.AddRange(
                new Category { CategoryId = CatId1, CategoryName = "Science", CreatedAt = DateTime.UtcNow },
                new Category { CategoryId = CatId2, CategoryName = "Math",    CreatedAt = DateTime.UtcNow }
            );

            _ctx.Quizzes.AddRange(
                new Quiz { QuizId = QuizId1, UserId = UserId1, CategoryId = CatId1, QuizName = "Science Quiz", Description = "d", PassMark = 5, TotalQuestion = 5, DifficultyLevel = "Easy", CreatedAt = DateTime.UtcNow },
                new Quiz { QuizId = QuizId2, UserId = UserId1, CategoryId = CatId2, QuizName = "Math Quiz",    Description = "d", PassMark = 5, TotalQuestion = 5, DifficultyLevel = "Easy", CreatedAt = DateTime.UtcNow }
            );

            // Attempts: alice 90%, bob 75%, carol 60% on Science
            //           alice 50% on Math
            _ctx.AttemptAnswers.AddRange(
                new AttemptAnswer { AttemptAnswerId = Guid.NewGuid(), UserId = UserId1, QuizId = QuizId1, TotalMark = 9,  Percentage = 90.0, CreatedAt = new DateTime(2026, 1, 1) },
                new AttemptAnswer { AttemptAnswerId = Guid.NewGuid(), UserId = UserId2, QuizId = QuizId1, TotalMark = 7,  Percentage = 75.0, CreatedAt = new DateTime(2026, 1, 2) },
                new AttemptAnswer { AttemptAnswerId = Guid.NewGuid(), UserId = UserId3, QuizId = QuizId1, TotalMark = 6,  Percentage = 60.0, CreatedAt = new DateTime(2026, 1, 3) },
                new AttemptAnswer { AttemptAnswerId = Guid.NewGuid(), UserId = UserId1, QuizId = QuizId2, TotalMark = 5,  Percentage = 50.0, CreatedAt = new DateTime(2026, 1, 4) }
            );

            _ctx.SaveChanges();
        }

        public void Dispose() => _ctx.Dispose();

        // ── GetTopScores — no filter ──────────────────────────────────────────

        [Fact]
        public async Task GetTopScores_NoFilter_ReturnsAllSortedByPercentage()
        {
            var result = await _svc.GetTopScores();

            Assert.Equal(4, result.Count);
            Assert.Equal(90.0, result[0].Percentage);
            Assert.Equal(75.0, result[1].Percentage);
            Assert.Equal(60.0, result[2].Percentage);
            Assert.Equal(50.0, result[3].Percentage);
        }

        [Fact]
        public async Task GetTopScores_NoFilter_FirstEntryHasCorrectUser()
        {
            var result = await _svc.GetTopScores();

            Assert.Equal(UserId1,      result[0].UserId);
            Assert.Equal("alice",      result[0].Username);
            Assert.Equal("Alice Smith",result[0].Name);
        }

        [Fact]
        public async Task GetTopScores_NoFilter_IncludesQuizName()
        {
            var result = await _svc.GetTopScores();

            Assert.Equal("Science Quiz", result[0].QuizName);
        }

        // ── GetTopScores — count limit ────────────────────────────────────────

        [Fact]
        public async Task GetTopScores_CountLimit_ReturnsOnlyTopN()
        {
            var result = await _svc.GetTopScores(count: 2);

            Assert.Equal(2, result.Count);
            Assert.Equal(90.0, result[0].Percentage);
            Assert.Equal(75.0, result[1].Percentage);
        }

        [Fact]
        public async Task GetTopScores_CountOne_ReturnsHighestOnly()
        {
            var result = await _svc.GetTopScores(count: 1);

            Assert.Single(result);
            Assert.Equal(90.0, result[0].Percentage);
            Assert.Equal("alice", result[0].Username);
        }

        // ── GetTopScores — category filter ────────────────────────────────────

        [Fact]
        public async Task GetTopScores_CategoryFilter_ReturnsOnlyThatCategory()
        {
            var result = await _svc.GetTopScores(categoryId: CatId1);

            Assert.Equal(3, result.Count);
            Assert.All(result, e => Assert.Equal(QuizId1, e.QuizId));
        }

        [Fact]
        public async Task GetTopScores_CategoryFilter_SortedByPercentage()
        {
            var result = await _svc.GetTopScores(categoryId: CatId1);

            Assert.Equal(90.0, result[0].Percentage);
            Assert.Equal(75.0, result[1].Percentage);
            Assert.Equal(60.0, result[2].Percentage);
        }

        [Fact]
        public async Task GetTopScores_MathCategory_ReturnsOnlyMathAttempts()
        {
            var result = await _svc.GetTopScores(categoryId: CatId2);

            Assert.Single(result);
            Assert.Equal(50.0,        result[0].Percentage);
            Assert.Equal("Math Quiz", result[0].QuizName);
        }

        [Fact]
        public async Task GetTopScores_UnknownCategory_ReturnsEmpty()
        {
            var result = await _svc.GetTopScores(categoryId: Guid.NewGuid());

            Assert.Empty(result);
        }

        // ── GetTopScores — tie-breaking ───────────────────────────────────────

        [Fact]
        public async Task GetTopScores_TiedPercentage_EarlierAttemptRanksFirst()
        {
            // Add two attempts with same percentage but different dates
            _ctx.AttemptAnswers.AddRange(
                new AttemptAnswer { AttemptAnswerId = Guid.NewGuid(), UserId = UserId2, QuizId = QuizId2, TotalMark = 8, Percentage = 80.0, CreatedAt = new DateTime(2026, 2, 1) },
                new AttemptAnswer { AttemptAnswerId = Guid.NewGuid(), UserId = UserId3, QuizId = QuizId2, TotalMark = 8, Percentage = 80.0, CreatedAt = new DateTime(2026, 2, 2) }
            );
            await _ctx.SaveChangesAsync();

            var result = await _svc.GetTopScores(categoryId: CatId2);

            // Both 80% — earlier date (Feb 1) should come first
            var tied = result.Where(r => r.Percentage == 80.0).ToList();
            Assert.Equal(2, tied.Count);
            Assert.Equal(UserId2, tied[0].UserId);
            Assert.Equal(UserId3, tied[1].UserId);
        }

        // ── GetTopScores — empty data ─────────────────────────────────────────

        [Fact]
        public async Task GetTopScores_NoAttempts_ReturnsEmpty()
        {
            var emptyOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var emptyCtx = new AppDbContext(emptyOptions);
            var emptySvc = new LeaderboardService(new Repository<Guid, AttemptAnswer>(emptyCtx));

            var result = await emptySvc.GetTopScores();

            Assert.Empty(result);
        }
    }
}
