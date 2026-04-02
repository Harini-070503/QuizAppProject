using Microsoft.EntityFrameworkCore;
using QuizAppProject.Context;
using QuizAppProject.Interfaces;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizAppProject.Services
{
    public class QuizService : IQuizService
    {
        private readonly IRepository<Guid, Quiz> _quizRepo;
        private readonly IRepository<Guid, User> _userRepo;
        private readonly IRepository<Guid, Category> _categoryRepo;
        private readonly IRepository<Guid, Question> _questionRepo;
        private readonly AppDbContext _db;

        public QuizService(
            IRepository<Guid, Quiz> quizRepo,
            IRepository<Guid, User> userRepo,
            IRepository<Guid, Category> categoryRepo,
            IRepository<Guid, Question> questionRepo,
            AppDbContext db)
        {
            _quizRepo = quizRepo;
            _userRepo = userRepo;
            _categoryRepo = categoryRepo;
            _questionRepo = questionRepo;
            _db = db;
        }

        public async Task<QuizDto> Add(Guid creatorId, QuizCreateDto request)
        {
            try
            {
                var creator = await _userRepo.Get(creatorId)
                    ?? throw new KeyNotFoundException("Creator not found.");
                if (!string.Equals(creator.Role, "Creator", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(creator.Role, "Evaluator", StringComparison.OrdinalIgnoreCase))
                    throw new UnauthorizedAccessException("Only creators and evaluators can create quizzes.");

                var category = await _categoryRepo.Get(request.CategoryId)
                    ?? throw new KeyNotFoundException("Category not found.");

                var quiz = new Quiz
                {
                    QuizId = Guid.NewGuid(),
                    UserId = creatorId,
                    CategoryId = category.CategoryId,
                    QuizName = request.QuizName,
                    Description = request.Description,
                    PassMark = request.PassMark,
                    TotalQuestion = request.TotalQuestion,
                    DifficultyLevel = request.DifficultyLevel,
                    TimeLimit = request.TimeLimit,
                    Deadline = request.Deadline,
                    CreatedAt = DateTime.UtcNow,
                    Questions = new List<Question>()
                };

                if (request.Questions != null && request.Questions.Any())
                {
                    foreach (var q in request.Questions)
                    {
                        quiz.Questions.Add(new Question
                        {
                            QuestionId = Guid.NewGuid(),
                            QuizId = quiz.QuizId,
                            QuestionText = q.QuestionText,
                            Marks = q.Marks > 0 ? q.Marks : 1,
                            CreatedAt = DateTime.UtcNow,
                            Options = new Option
                            {
                                OptionId = Guid.NewGuid(),
                                OptionA = q.Options.OptionA,
                                OptionB = q.Options.OptionB,
                                OptionC = q.Options.OptionC,
                                OptionD = q.Options.OptionD,
                                CorrectOption = q.Options.CorrectOption,
                                CreatedAt = DateTime.UtcNow
                            }
                        });
                    }
                }

                var added = await _quizRepo.Add(quiz);
                if (added is null)
                    throw new InvalidOperationException("Failed to create quiz.");

                return await MapQuizDto(quiz.QuizId);
            }
            catch (KeyNotFoundException ex) { throw new KeyNotFoundException(ex.Message); }
            catch (UnauthorizedAccessException) { throw; }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while creating the quiz.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while creating the quiz.", ex);
            }
        }

        public async Task<QuizDto?> Get(Guid quizId)
        {
            // Keeping your original behavior (return null on any error).
            try { return await MapQuizDto(quizId); }
            catch { return null; }
        }

        public async Task<List<QuizDto>> GetAll(Guid? categoryId = null, Guid? requestingUserId = null)
        {
            try
            {
                var query = _quizRepo.Query()
                    .Include(q => q.Category)
                    .Include(q => q.User).ThenInclude(u => u.UserDetails)
                    .Include(q => q.Questions).ThenInclude(qq => qq.Options)
                    .AsNoTracking();

                if (categoryId.HasValue)
                    query = query.Where(q => q.CategoryId == categoryId.Value);

                var list = await query.ToListAsync();

                // Get group memberships for the requesting user (if provided)
                HashSet<Guid> memberGroupIds = new();
                if (requestingUserId.HasValue)
                {
                    memberGroupIds = (await _db.QuizGroupMembers
                        .Where(m => m.UserId == requestingUserId.Value)
                        .Select(m => m.GroupId)
                        .ToListAsync()).ToHashSet();
                }

                var result = new List<QuizDto>();
                foreach (var q in list)
                {
                    var creatorRole = q.User?.Role ?? "";
                    bool isEvaluatorQuiz = string.Equals(creatorRole, "Evaluator", StringComparison.OrdinalIgnoreCase);

                    if (isEvaluatorQuiz)
                    {
                        // Evaluator quiz: only visible if:
                        // 1. No group assigned (open evaluator quiz — still restricted, skip)
                        // 2. Requesting user is the evaluator themselves
                        // 3. Requesting user is a member of the assigned group
                        if (!requestingUserId.HasValue) continue; // anonymous — hide all evaluator quizzes

                        bool isOwner = q.UserId == requestingUserId.Value;
                        bool isMember = q.GroupId.HasValue && memberGroupIds.Contains(q.GroupId.Value);

                        if (!isOwner && !isMember) continue; // not accessible
                    }

                    result.Add(new QuizDto
                    {
                        QuizId = q.QuizId,
                        QuizName = q.QuizName,
                        Description = q.Description,
                        DifficultyLevel = q.DifficultyLevel,
                        TimeLimit = q.TimeLimit,
                        Deadline = q.Deadline,
                        PassMark = q.PassMark,
                        TotalQuestion = q.TotalQuestion,
                        Category = new CategoryDto { CategoryId = q.CategoryId ?? Guid.Empty, CategoryName = q.Category?.CategoryName ?? "Uncategorized" },
                        CreatorId = q.UserId,
                        GroupId = q.GroupId,
                        CreatorRole = creatorRole,
                        CreatorName = q.User?.UserDetails?.Name ?? q.User?.Username,
                        Questions = q.Questions.Select(qq => new QuestionDto
                        {
                            QuestionId = qq.QuestionId,
                            QuestionText = qq.QuestionText,
                            Marks = qq.Marks,
                            Options = new OptionDto
                            {
                                OptionId = qq.Options.OptionId,
                                OptionA = qq.Options.OptionA,
                                OptionB = qq.Options.OptionB,
                                OptionC = qq.Options.OptionC,
                                OptionD = qq.Options.OptionD,
                                CorrectOption = qq.Options.CorrectOption
                            }
                        }).ToList()
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while fetching quizzes.", ex);
            }
        }

        public async Task<QuizDto> Update(Guid quizId, QuizUpdateDto request, Guid creatorId)
        {
            try
            {
                // Load quiz for ownership check and scalar updates
                var quiz = await _quizRepo.Query()
                    .Include(q => q.User)
                    .Include(q => q.Category)
                    .FirstOrDefaultAsync(q => q.QuizId == quizId)
                    ?? throw new KeyNotFoundException("Quiz not found.");

                if (quiz.UserId != creatorId)
                    throw new UnauthorizedAccessException("Only the creator or evaluator can update this quiz.");

                // Update scalar fields
                quiz.QuizName = request.QuizName;
                quiz.Description = request.Description;
                quiz.PassMark = request.PassMark;
                quiz.TotalQuestion = request.TotalQuestion;
                quiz.DifficultyLevel = request.DifficultyLevel;
                quiz.TimeLimit = request.TimeLimit;
                quiz.Deadline = request.Deadline;
                quiz.CategoryId = request.CategoryId;

                // Persist scalar changes
                var updated = await _quizRepo.Update(quizId, quiz);
                if (updated is null)
                    throw new InvalidOperationException("Failed to update quiz.");

                // Replace questions: delete existing then insert new set
                var existingQuestionIds = await _questionRepo.Query()
                    .Where(x => x.QuizId == quizId)
                    .Select(x => x.QuestionId)
                    .ToListAsync();

                foreach (var qid in existingQuestionIds)
                    await _questionRepo.Delete(qid);

                if (request.Questions != null && request.Questions.Any())
                {
                    foreach (var q in request.Questions)
                    {
                        var question = new Question
                        {
                            QuestionId = Guid.NewGuid(),
                            QuizId = quizId,
                            QuestionText = q.QuestionText,
                            Marks = q.Marks > 0 ? q.Marks : 1,
                            CreatedAt = DateTime.UtcNow,
                            Options = new Option
                            {
                                OptionId = Guid.NewGuid(),
                                OptionA = q.Options.OptionA,
                                OptionB = q.Options.OptionB,
                                OptionC = q.Options.OptionC,
                                OptionD = q.Options.OptionD,
                                CorrectOption = q.Options.CorrectOption,
                                CreatedAt = DateTime.UtcNow
                            }
                        };

                        var addedQuestion = await _questionRepo.Add(question);
                        if (addedQuestion is null)
                            throw new InvalidOperationException("Failed to add a question during quiz update.");
                    }
                }

                return await MapQuizDto(quizId);
            }
            catch (KeyNotFoundException) { throw; }
            catch (UnauthorizedAccessException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while updating the quiz.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while updating the quiz.", ex);
            }
        }

        public async Task<bool> Delete(Guid quizId, Guid creatorId)
        {
            try
            {
                var quiz = await _quizRepo.Get(quizId);
                if (quiz == null) return false;

                if (quiz.UserId != creatorId)
                    throw new UnauthorizedAccessException("Only the creator can delete this quiz.");

                // If cascade delete is configured for Questions/Options, this will remove all children.
                var deleted = await _quizRepo.Delete(quizId);
                return deleted != null;
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while deleting the quiz.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while deleting the quiz.", ex);
            }
        }

        // ----- Helpers -----
        private async Task<QuizDto> MapQuizDto(Guid quizId)
        {
            try
            {
                var q = await _quizRepo.Query()
                    .Include(q => q.Category)
                    .Include(q => q.User).ThenInclude(u => u.UserDetails)
                    .Include(q => q.Questions).ThenInclude(qq => qq.Options)
                    .FirstOrDefaultAsync(q => q.QuizId == quizId)
                    ?? throw new KeyNotFoundException("Quiz not found.");

                return new QuizDto
                {
                    QuizId = q.QuizId,
                    QuizName = q.QuizName,
                    Description = q.Description,
                    DifficultyLevel = q.DifficultyLevel,
                    TimeLimit = q.TimeLimit,
                    Deadline = q.Deadline,
                    PassMark = q.PassMark,
                    TotalQuestion = q.TotalQuestion,
                    Category = new CategoryDto { CategoryId = q.CategoryId ?? Guid.Empty, CategoryName = q.Category?.CategoryName ?? "Uncategorized" },
                    CreatorId = q.UserId,
                    GroupId = q.GroupId,
                    CreatorName = q.User?.UserDetails?.Name ?? q.User?.Username,
                    Questions = q.Questions.Select(qq => new QuestionDto
                    {
                        QuestionId = qq.QuestionId,
                        QuestionText = qq.QuestionText,
                        Marks = qq.Marks,
                        Options = new OptionDto
                        {
                            OptionId = qq.Options.OptionId,
                            OptionA = qq.Options.OptionA,
                            OptionB = qq.Options.OptionB,
                            OptionC = qq.Options.OptionC,
                            OptionD = qq.Options.OptionD,
                            CorrectOption = qq.Options.CorrectOption
                        }
                    }).ToList()
                };
            }
            catch (KeyNotFoundException) { throw; }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while mapping quiz details.", ex);
            }
        }
    }
}

