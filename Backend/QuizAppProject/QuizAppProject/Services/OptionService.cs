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
    public class OptionService : IOptionService
    {
        private readonly IRepository<Guid, Option> _optionRepo;
        private readonly IRepository<Guid, Question> _questionRepo;

        public OptionService(
            IRepository<Guid, Option> optionRepo,
            IRepository<Guid, Question> questionRepo)
        {
            _optionRepo = optionRepo;
            _questionRepo = questionRepo;
        }

        public async Task<OptionDto> Add(Guid questionId, OptionCreateDto request)
        {
            try
            {
                // Load Question with its Options so we can ensure only one Option record per Question
                var q = await _questionRepo.Query()
                    .Include(x => x.Options)
                    .FirstOrDefaultAsync(x => x.QuestionId == questionId)
                    ?? throw new KeyNotFoundException("Question not found.");

                if (q.Options != null)
                    throw new InvalidOperationException("Options already exist for this question.");

                var opt = new Option
                {
                    OptionId = Guid.NewGuid(),
                    QuestionId = q.QuestionId,
                    OptionA = request.OptionA,
                    OptionB = request.OptionB,
                    OptionC = request.OptionC,
                    OptionD = request.OptionD,
                    CorrectOption = request.CorrectOption,
                    CreatedAt = DateTime.UtcNow
                };

                var added = await _optionRepo.Add(opt);
                if (added is null)
                    throw new InvalidOperationException("Failed to create options.");

                return new OptionDto
                {
                    OptionId = opt.OptionId,
                    OptionA = opt.OptionA,
                    OptionB = opt.OptionB,
                    OptionC = opt.OptionC,
                    OptionD = opt.OptionD,
                    CorrectOption = opt.CorrectOption
                };
            }
            catch (KeyNotFoundException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while creating options.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while creating options.", ex);
            }
        }

        public async Task<OptionDto?> Get(Guid optionId)
        {
            try
            {
                var opt = await _optionRepo.Get(optionId);
                if (opt == null) return null;

                return new OptionDto
                {
                    OptionId = opt.OptionId,
                    OptionA = opt.OptionA,
                    OptionB = opt.OptionB,
                    OptionC = opt.OptionC,
                    OptionD = opt.OptionD,
                    CorrectOption = opt.CorrectOption
                };
            }
            catch (Exception ex)
            {
                // Preserve nullable behavior; add context for upstream handlers/logging
                throw new Exception("Unexpected error while fetching options.", ex);
            }
        }

        public async Task<OptionDto> Update(Guid optionId, OptionCreateDto request)
        {
            try
            {
                var opt = await _optionRepo.Get(optionId)
                    ?? throw new KeyNotFoundException("Option not found.");

                opt.OptionA = request.OptionA;
                opt.OptionB = request.OptionB;
                opt.OptionC = request.OptionC;
                opt.OptionD = request.OptionD;
                opt.CorrectOption = request.CorrectOption;

                var updated = await _optionRepo.Update(optionId, opt);
                if (updated is null)
                    throw new InvalidOperationException("Failed to update options.");

                return new OptionDto
                {
                    OptionId = opt.OptionId,
                    OptionA = opt.OptionA,
                    OptionB = opt.OptionB,
                    OptionC = opt.OptionC,
                    OptionD = opt.OptionD,
                    CorrectOption = opt.CorrectOption
                };
            }
            catch (KeyNotFoundException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while updating options.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while updating options.", ex);
            }
        }

        public async Task<bool> Delete(Guid optionId)
        {
            try
            {
                var deleted = await _optionRepo.Delete(optionId);
                return deleted != null;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while deleting options.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while deleting options.", ex);
            }
        }
    }
}