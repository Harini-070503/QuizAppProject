using Microsoft.EntityFrameworkCore;
// ✅ DTOs & Interfaces
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
    public class AttemptService : IAttemptService
    {
        private readonly IRepository<Guid, Quiz> _quizRepo;
        private readonly IRepository<Guid, AttemptAnswer> _attemptRepo;
        private readonly AppDbContext _db;

        public AttemptService(
            IRepository<Guid, Quiz> quizRepo,
            IRepository<Guid, AttemptAnswer> attemptRepo,
            AppDbContext db)
        {
            _quizRepo = quizRepo;
            _attemptRepo = attemptRepo;
            _db = db;
        }

        public async Task<AttemptResultDto> SubmitAttempt(Guid userId, AttemptSubmitDto request)
        {
            try
            {
                // Load quiz (with Questions, Options, and Creator role)
                var quiz = await _quizRepo.Query()
                    .Include(q => q.Questions)
                        .ThenInclude(qq => qq.Options)
                    .Include(q => q.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(q => q.QuizId == request.QuizId)
                    ?? throw new KeyNotFoundException("Quiz not found.");

                // Determine if this is an evaluator quiz (manual grading only)
                bool isEvaluatorQuiz = string.Equals(quiz.User?.Role, "Evaluator", StringComparison.OrdinalIgnoreCase);

                // Validate time limit if provided
                if (quiz.TimeLimit.HasValue && request.StartedAtUtc.HasValue && request.EndedAtUtc.HasValue)
                {
                    var elapsed = (request.EndedAtUtc.Value - request.StartedAtUtc.Value).TotalMinutes;
                    if (elapsed - 0.1 > quiz.TimeLimit.Value) // ~6 sec grace
                        throw new InvalidOperationException("Time limit exceeded.");
                }

                var answersMap = request.Answers.ToDictionary(a => a.QuestionId, a => a.ChosenOption);
                var feedback = new List<AttemptFeedbackItemDto>();
                var detailsToAdd = new List<AttemptAnswerDetail>();

                int correct = 0;
                int totalPossible = 0;
                foreach (var question in quiz.Questions)
                {
                    answersMap.TryGetValue(question.QuestionId, out var chosen);
                    var correctOpt = question.Options?.CorrectOption ?? "";
                    var qMarks = question.Marks > 0 ? question.Marks : 1;
                    totalPossible += qMarks;

                    bool isCorrect = false;
                    int marksAwarded = 0;

                    if (!isEvaluatorQuiz)
                    {
                        isCorrect = !string.IsNullOrWhiteSpace(chosen)
                                    && string.Equals(chosen, correctOpt, StringComparison.OrdinalIgnoreCase);
                        if (isCorrect) { correct += qMarks; marksAwarded = qMarks; }
                    }
                    else
                    {
                        // For evaluator quizzes: detect correctness for display, but marks = 0 until manually graded
                        isCorrect = !string.IsNullOrWhiteSpace(chosen)
                                    && string.Equals(chosen, correctOpt, StringComparison.OrdinalIgnoreCase);
                        marksAwarded = 0; // evaluator assigns marks manually
                    }

                    feedback.Add(new AttemptFeedbackItemDto
                    {
                        QuestionId = question.QuestionId,
                        CorrectOption = correctOpt,
                        YourOption = chosen,
                        IsCorrect = isCorrect
                    });

                    detailsToAdd.Add(new AttemptAnswerDetail
                    {
                        Id = Guid.NewGuid(),
                        AttemptAnswerId = Guid.Empty, // set after attempt saved
                        QuestionId = question.QuestionId,
                        ChosenOption = chosen,
                        IsCorrect = isCorrect,
                        MarksAwarded = marksAwarded,
                        MaxMarks = qMarks
                    });
                }

                var totalMark = isEvaluatorQuiz ? 0 : correct;
                var percentage = isEvaluatorQuiz ? 0 :
                    (totalPossible > 0 ? (double)correct / totalPossible * 100.0 : 0);

                var attempt = new AttemptAnswer
                {
                    AttemptAnswerId = Guid.NewGuid(),
                    UserId = userId,
                    QuizId = quiz.QuizId,
                    TotalMark = totalMark,
                    Percentage = Math.Round(percentage, 2),
                    CreatedAt = DateTime.UtcNow
                };

                var added = await _attemptRepo.Add(attempt);
                if (added is null)
                    throw new InvalidOperationException("Failed to submit attempt.");

                // Now set the real AttemptAnswerId and save details
                foreach (var detail in detailsToAdd)
                {
                    detail.AttemptAnswerId = attempt.AttemptAnswerId;
                    _db.AttemptAnswerDetails.Add(detail);
                }

                // Notify the evaluator (quiz creator) that a student finished the test
                if (isEvaluatorQuiz)
                {
                    var student = await _db.Users.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserId == userId);

                    _db.Notifications.Add(new Notification
                    {
                        NotificationId = Guid.NewGuid(),
                        UserId = quiz.UserId,   // evaluator
                        Type = "test_submitted",
                        Title = "Student Completed a Test",
                        Message = $"{student?.Username ?? "A student"} has completed the test \"{quiz.QuizName}\". Review and grade their submission.",
                        LinkUrl = $"/evaluator/submission/{attempt.AttemptAnswerId}",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _db.SaveChangesAsync();

                return new AttemptResultDto
                {
                    AttemptAnswerId = attempt.AttemptAnswerId,
                    QuizId = quiz.QuizId,
                    TotalMark = totalMark,
                    Percentage = attempt.Percentage,
                    IsPendingEvaluation = isEvaluatorQuiz,
                    Feedback = feedback
                };
            }
            catch (KeyNotFoundException) { throw; }          // preserve 404 semantics
            catch (InvalidOperationException) { throw; }     // business rule or failed add
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while submitting the attempt.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while submitting the attempt.", ex);
            }
        }

        public async Task<List<AttemptResultDto>> GetAttemptsByUser(Guid userId)
        {
            try
            {
                var attempts = await _attemptRepo.Query()
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();

                return attempts.Select(a => new AttemptResultDto
                {
                    AttemptAnswerId = a.AttemptAnswerId,
                    QuizId = a.QuizId,
                    TotalMark = a.TotalMark,
                    Percentage = a.Percentage,
                    Feedback = new List<AttemptFeedbackItemDto>() // not storing per-question history here
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while fetching attempts for the user.", ex);
            }
        }

        public async Task<AttemptResultDto?> GetAttempt(Guid attemptId)
        {
            try
            {
                var a = await _db.AttemptAnswers
                    .Include(x => x.Details).ThenInclude(d => d.Question).ThenInclude(q => q.Options)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.AttemptAnswerId == attemptId);

                if (a == null) return null;

                var feedback = a.Details.Select(d => new AttemptFeedbackItemDto
                {
                    QuestionId = d.QuestionId,
                    CorrectOption = d.Question?.Options?.CorrectOption ?? "",
                    YourOption = d.ChosenOption,
                    IsCorrect = d.IsCorrect
                }).ToList();

                return new AttemptResultDto
                {
                    AttemptAnswerId = a.AttemptAnswerId,
                    QuizId = a.QuizId,
                    TotalMark = a.TotalMark,
                    Percentage = a.Percentage,
                    Feedback = feedback
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while fetching the attempt.", ex);
            }
        }

        public async Task<List<SubmissionDetailDto>> GetSubmissionsByQuiz(Guid quizId)
        {
            var attempts = await _db.AttemptAnswers
                .Include(a => a.User)
                .Include(a => a.Details)
                .Where(a => a.QuizId == quizId)
                .OrderByDescending(a => a.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var quiz = await _db.Quizzes.AsNoTracking().FirstOrDefaultAsync(q => q.QuizId == quizId);

            return attempts.Select(a => new SubmissionDetailDto
            {
                AttemptAnswerId = a.AttemptAnswerId,
                QuizId = a.QuizId,
                QuizName = quiz?.QuizName ?? "",
                UserId = a.UserId,
                Username = a.User?.Username ?? "",
                TotalMark = a.TotalMark,
                MaxMark = a.Details.Sum(d => d.MaxMarks),
                Percentage = a.Percentage,
                SubmittedAt = a.CreatedAt,
                Answers = new List<SubmissionAnswerDto>()
            }).ToList();
        }

        public async Task<SubmissionDetailDto?> GetSubmissionDetail(Guid attemptId)
        {
            var a = await _db.AttemptAnswers
                .Include(x => x.User)
                .Include(x => x.Quiz)
                .Include(x => x.Details).ThenInclude(d => d.Question).ThenInclude(q => q.Options)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AttemptAnswerId == attemptId);

            if (a == null) return null;

            return new SubmissionDetailDto
            {
                AttemptAnswerId = a.AttemptAnswerId,
                QuizId = a.QuizId,
                QuizName = a.Quiz?.QuizName ?? "",
                UserId = a.UserId,
                Username = a.User?.Username ?? "",
                TotalMark = a.TotalMark,
                MaxMark = a.Details.Sum(d => d.MaxMarks),
                Percentage = a.Percentage,
                SubmittedAt = a.CreatedAt,
                Answers = a.Details.Select(d => new SubmissionAnswerDto
                {
                    QuestionId = d.QuestionId,
                    QuestionText = d.Question?.QuestionText ?? "",
                    ChosenOption = d.ChosenOption,
                    CorrectOption = d.Question?.Options?.CorrectOption ?? "",
                    IsCorrect = d.IsCorrect,
                    MarksAwarded = d.MarksAwarded,
                    MaxMarks = d.MaxMarks
                }).ToList()
            };
        }

        public async Task<bool> UpdateScore(Guid attemptId, UpdateScoreDto dto)
        {
            var attempt = await _db.AttemptAnswers
                .Include(x => x.Details)
                .Include(x => x.Quiz)
                .FirstOrDefaultAsync(x => x.AttemptAnswerId == attemptId);

            if (attempt == null) return false;

            foreach (var qs in dto.QuestionScores)
            {
                var detail = attempt.Details.FirstOrDefault(d => d.QuestionId == qs.QuestionId);
                if (detail != null) detail.MarksAwarded = qs.MarksAwarded;
            }

            attempt.TotalMark = dto.NewTotalMark;
            var maxMark = attempt.Details.Sum(d => d.MaxMarks);
            attempt.Percentage = maxMark > 0 ? Math.Round((double)dto.NewTotalMark / maxMark * 100, 2) : 0;

            // Notify the student that their test has been graded
            _db.Notifications.Add(new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = attempt.UserId,
                Type = "score_updated",
                Title = "Your Test Has Been Graded",
                Message = $"Your test \"{attempt.Quiz?.QuizName ?? ""}\" has been graded. You scored {dto.NewTotalMark}/{maxMark} ({attempt.Percentage}%).",
                LinkUrl = $"/result/{attempt.AttemptAnswerId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return true;
        }
    }
}