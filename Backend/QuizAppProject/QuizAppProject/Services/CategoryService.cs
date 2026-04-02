using Microsoft.EntityFrameworkCore;
using QuizAppProject.Interfaces;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizAppProject.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Guid, Category> _categoryRepo;

        public CategoryService(IRepository<Guid, Category> categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<CategoryDto> Add(CategoryCreateDto request)
        {
            try
            {
                // Uniqueness check via repository Query()
                var exists = await _categoryRepo.Query()
                    .AnyAsync(c => c.CategoryName == request.CategoryName);

                if (exists)
                    throw new InvalidOperationException("Category already exists.");

                var c = new Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = request.CategoryName,
                    CreatedAt = DateTime.UtcNow
                };

                var added = await _categoryRepo.Add(c);
                if (added is null)
                    throw new InvalidOperationException("Failed to create category.");

                return new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                };
            }
            catch (InvalidOperationException) { throw; } // business rule (duplicate) or failed create
            catch (DbUpdateException ex)
            {
                // Wrap persistence errors with a domain-friendly message
                throw new InvalidOperationException("A database error occurred while creating the category.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while creating the category.", ex);
            }
        }

        public async Task<CategoryDto?> Get(Guid id)
        {
            try
            {
                var c = await _categoryRepo.Get(id);
                if (c == null) return null;

                return new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                };
            }
            catch (Exception ex)
            {
                // Keep nullable contract; unexpected issues still bubble up for controller to handle
                throw new Exception("Unexpected error while fetching the category.", ex);
            }
        }

        public async Task<List<CategoryDto>> GetAll()
        {
            try
            {
                var list = await _categoryRepo.Query()
                    .AsNoTracking()
                    .ToListAsync();

                return list.Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while fetching categories.", ex);
            }
        }

        public async Task<CategoryDto> Update(Guid id, CategoryCreateDto request)
        {
            try
            {
                // Load existing entity
                var existing = await _categoryRepo.Get(id)
                    ?? throw new KeyNotFoundException("Category not found.");

                // Enforce unique name on update (excluding current record)
                var nameTaken = await _categoryRepo.Query()
                    .AnyAsync(c => c.CategoryName == request.CategoryName && c.CategoryId != id);
                if (nameTaken)
                    throw new InvalidOperationException("Another category with the same name already exists.");

                existing.CategoryName = request.CategoryName;

                var updated = await _categoryRepo.Update(id, existing);
                if (updated is null)
                    throw new InvalidOperationException("Failed to update category.");

                return new CategoryDto
                {
                    CategoryId = existing.CategoryId,
                    CategoryName = existing.CategoryName
                };
            }
            catch (KeyNotFoundException) { throw; }        // keep 404 semantics for controller
            catch (InvalidOperationException) { throw; }   // duplicate / failed update -> 409/400 by controller
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while updating the category.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while updating the category.", ex);
            }
        }

        public async Task<bool> Delete(Guid id)
        {
            try
            {
                var deleted = await _categoryRepo.Delete(id);
                return deleted != null;
            }
            catch (DbUpdateException ex)
            {
                // For FK constraints etc.
                throw new InvalidOperationException("Unable to delete the category due to related data.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while deleting the category.", ex);
            }
        }
    }
}
