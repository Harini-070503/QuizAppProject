using Microsoft.EntityFrameworkCore;
using QuizAppProject.Context;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;
using QuizAppProject.Repositories;
using QuizAppProject.Services;

namespace Testing.Services
{
    public class CategoryServiceTests : IDisposable
    {
        private readonly AppDbContext _ctx;
        private readonly CategoryService _svc;

        private static readonly Guid CatId1 = Guid.Parse("ca100000-0000-0000-0000-000000000001");
        private static readonly Guid CatId2 = Guid.Parse("ca100000-0000-0000-0000-000000000002");
        private static readonly Guid CatId3 = Guid.Parse("ca100000-0000-0000-0000-000000000003");

        public CategoryServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _ctx = new AppDbContext(options);
            SeedData();

            var categoryRepo = new Repository<Guid, Category>(_ctx);
            _svc = new CategoryService(categoryRepo);
        }

        private void SeedData()
        {
            _ctx.Categories.AddRange(
                new Category { CategoryId = CatId1, CategoryName = "Science",  CreatedAt = DateTime.UtcNow },
                new Category { CategoryId = CatId2, CategoryName = "Math",     CreatedAt = DateTime.UtcNow },
                new Category { CategoryId = CatId3, CategoryName = "History",  CreatedAt = DateTime.UtcNow }
            );
            _ctx.SaveChanges();
        }

        public void Dispose() => _ctx.Dispose();

        // ── Get ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Get_ExistingCategory_ReturnsDto()
        {
            var result = await _svc.Get(CatId1);

            Assert.NotNull(result);
            Assert.Equal(CatId1,     result!.CategoryId);
            Assert.Equal("Science",  result.CategoryName);
        }

        [Fact]
        public async Task Get_UnknownCategory_ReturnsNull()
        {
            var result = await _svc.Get(Guid.NewGuid());
            Assert.Null(result);
        }

        // ── GetAll ────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_ReturnsAllCategories()
        {
            var result = await _svc.GetAll();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAll_ContainsCorrectNames()
        {
            var result = await _svc.GetAll();
            var names  = result.Select(c => c.CategoryName).ToList();

            Assert.Contains("Science", names);
            Assert.Contains("Math",    names);
            Assert.Contains("History", names);
        }

        [Fact]
        public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
        {
            var emptyOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var emptyCtx = new AppDbContext(emptyOptions);
            var emptySvc = new CategoryService(new Repository<Guid, Category>(emptyCtx));

            var result = await emptySvc.GetAll();

            Assert.Empty(result);
        }

        // ── Add ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Add_NewCategory_ReturnsDto()
        {
            var result = await _svc.Add(new CategoryCreateDto { CategoryName = "Physics" });

            Assert.NotNull(result);
            Assert.Equal("Physics", result.CategoryName);
            Assert.NotEqual(Guid.Empty, result.CategoryId);
        }

        [Fact]
        public async Task Add_NewCategory_AppearsInGetAll()
        {
            await _svc.Add(new CategoryCreateDto { CategoryName = "Chemistry" });

            var all = await _svc.GetAll();
            Assert.Equal(4, all.Count);
            Assert.Contains(all, c => c.CategoryName == "Chemistry");
        }

        [Fact]
        public async Task Add_DuplicateName_ThrowsInvalidOperation()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _svc.Add(new CategoryCreateDto { CategoryName = "Science" }));
        }

        [Fact]
        public async Task Add_DuplicateNameCaseSensitive_ThrowsInvalidOperation()
        {
            // Exact duplicate — same case
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _svc.Add(new CategoryCreateDto { CategoryName = "Math" }));
        }

        // ── Update ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Update_ValidRequest_UpdatesName()
        {
            var result = await _svc.Update(CatId1, new CategoryCreateDto { CategoryName = "Advanced Science" });

            Assert.Equal("Advanced Science", result.CategoryName);
            Assert.Equal(CatId1, result.CategoryId);
        }

        [Fact]
        public async Task Update_SameNameSameId_Succeeds()
        {
            // Updating with the same name (no conflict with itself)
            var result = await _svc.Update(CatId1, new CategoryCreateDto { CategoryName = "Science" });

            Assert.Equal("Science", result.CategoryName);
        }

        [Fact]
        public async Task Update_NameTakenByOther_ThrowsInvalidOperation()
        {
            // CatId1 = "Science", trying to rename to "Math" (CatId2)
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _svc.Update(CatId1, new CategoryCreateDto { CategoryName = "Math" }));
        }

        [Fact]
        public async Task Update_UnknownCategory_ThrowsKeyNotFoundException()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.Update(Guid.NewGuid(), new CategoryCreateDto { CategoryName = "New Name" }));
        }

        [Fact]
        public async Task Update_PersistsInGetAll()
        {
            await _svc.Update(CatId2, new CategoryCreateDto { CategoryName = "Applied Math" });

            var all = await _svc.GetAll();
            Assert.Contains(all, c => c.CategoryName == "Applied Math");
            Assert.DoesNotContain(all, c => c.CategoryName == "Math");
        }

        // ── Delete ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_ExistingCategory_ReturnsTrue()
        {
            var result = await _svc.Delete(CatId3);

            Assert.True(result);
        }

        [Fact]
        public async Task Delete_ExistingCategory_RemovedFromGetAll()
        {
            await _svc.Delete(CatId3);

            var all = await _svc.GetAll();
            Assert.Equal(2, all.Count);
            Assert.DoesNotContain(all, c => c.CategoryId == CatId3);
        }

        [Fact]
        public async Task Delete_UnknownCategory_ReturnsFalse()
        {
            var result = await _svc.Delete(Guid.NewGuid());
            Assert.False(result);
        }

        [Fact]
        public async Task Delete_DoesNotAffectOtherCategories()
        {
            await _svc.Delete(CatId1);

            var all = await _svc.GetAll();
            Assert.Contains(all, c => c.CategoryId == CatId2);
            Assert.Contains(all, c => c.CategoryId == CatId3);
        }
    }
}
