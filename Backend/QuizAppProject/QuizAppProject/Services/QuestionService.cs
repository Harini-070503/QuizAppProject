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
    public class QuestionService : IQuestionService
    {
        private readonly IRepository<Guid, Question> _questionRepo;
        private readonly IRepository<Guid, Quiz> _quizRepo;

        public QuestionService(
            IRepository<Guid, Question> questionRepo,
            IRepository<Guid, Quiz> quizRepo)
        {
            _questionRepo = questionRepo;
            _quizRepo = quizRepo;
        }

        public async Task<QuestionDto> Add(Guid quizId, QuestionCreateDto request)
        {
            try
            {
                var quiz = await _quizRepo.Get(quizId)
                    ?? throw new KeyNotFoundException("Quiz not found.");

                var q = new Question
                {
                    QuestionId = Guid.NewGuid(),
                    QuizId = quiz.QuizId,
                    QuestionText = request.QuestionText,
                    Marks = request.Marks > 0 ? request.Marks : 1,
                    CreatedAt = DateTime.UtcNow,
                    Options = new Option
                    {
                        OptionId = Guid.NewGuid(),
                        OptionA = request.Options.OptionA,
                        OptionB = request.Options.OptionB,
                        OptionC = request.Options.OptionC,
                        OptionD = request.Options.OptionD,
                        CorrectOption = request.Options.CorrectOption,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                var added = await _questionRepo.Add(q);
                if (added is null)
                    throw new InvalidOperationException("Failed to create question.");

                return new QuestionDto
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    Marks = q.Marks,
                    Options = new OptionDto
                    {
                        OptionId = q.Options.OptionId,
                        OptionA = q.Options.OptionA,
                        OptionB = q.Options.OptionB,
                        OptionC = q.Options.OptionC,
                        OptionD = q.Options.OptionD,
                        CorrectOption = q.Options.CorrectOption
                    }
                };
            }
            catch (KeyNotFoundException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while creating the question.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while creating the question.", ex);
            }
        }

        public async Task<QuestionDto?> Get(Guid questionId)
        {
            try
            {
                var q = await _questionRepo.Query()
                    .Include(x => x.Options)
                    .FirstOrDefaultAsync(x => x.QuestionId == questionId);

                if (q == null) return null;

                return new QuestionDto
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    Marks = q.Marks,
                    Options = new OptionDto
                    {
                        OptionId = q.Options.OptionId,
                        OptionA = q.Options.OptionA,
                        OptionB = q.Options.OptionB,
                        OptionC = q.Options.OptionC,
                        OptionD = q.Options.OptionD,
                        CorrectOption = q.Options.CorrectOption
                    }
                };
            }
            catch (Exception ex)
            {
                // Keep nullable contract; bubble with context for controller mapping/logging
                throw new Exception("Unexpected error while fetching the question.", ex);
            }
        }

        public async Task<List<QuestionDto>> GetByQuiz(Guid quizId)
        {
            try
            {
                var list = await _questionRepo.Query()
                    .Include(q => q.Options)
                    .Where(q => q.QuizId == quizId)
                    .ToListAsync();

                return list.Select(q => new QuestionDto
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    Marks = q.Marks,
                    Options = new OptionDto
                    {
                        OptionId = q.Options.OptionId,
                        OptionA = q.Options.OptionA,
                        OptionB = q.Options.OptionB,
                        OptionC = q.Options.OptionC,
                        OptionD = q.Options.OptionD,
                        CorrectOption = q.Options.CorrectOption
                    }
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while fetching questions for the quiz.", ex);
            }
        }

        public async Task<QuestionDto> Update(Guid questionId, QuestionCreateDto request)
        {
            try
            {
                var q = await _questionRepo.Query()
                    .Include(x => x.Options)
                    .FirstOrDefaultAsync(x => x.QuestionId == questionId)
                    ?? throw new KeyNotFoundException("Question not found.");

                q.QuestionText = request.QuestionText;
                q.Marks = request.Marks > 0 ? request.Marks : 1;

                if (q.Options == null)
                {
                    q.Options = new Option
                    {
                        OptionId = Guid.NewGuid(),
                        CreatedAt = DateTime.UtcNow
                    };
                }

                q.Options.OptionA = request.Options.OptionA;
                q.Options.OptionB = request.Options.OptionB;
                q.Options.OptionC = request.Options.OptionC;
                q.Options.OptionD = request.Options.OptionD;
                q.Options.CorrectOption = request.Options.CorrectOption;

                var updated = await _questionRepo.Update(questionId, q);
                if (updated is null)
                    throw new InvalidOperationException("Failed to update question.");

                return new QuestionDto
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    Marks = q.Marks,
                    Options = new OptionDto
                    {
                        OptionId = q.Options.OptionId,
                        OptionA = q.Options.OptionA,
                        OptionB = q.Options.OptionB,
                        OptionC = q.Options.OptionC,
                        OptionD = q.Options.OptionD,
                        CorrectOption = q.Options.CorrectOption
                    }
                };
            }
            catch (KeyNotFoundException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while updating the question.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while updating the question.", ex);
            }
        }

        public async Task<bool> Delete(Guid questionId)
        {
            try
            {
                var deleted = await _questionRepo.Delete(questionId);
                return deleted != null;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while deleting the question.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while deleting the question.", ex);
            }
        }
    }
}
