using System.ComponentModel.DataAnnotations;

namespace QuizAppProject.Models.DTOs
{

    public class AttemptSubmitDto
    {
        [Required] public Guid QuizId { get; set; }
        [Required] public Guid UserId { get; set; }

        [Required] public List<AttemptAnswerItemDto> Answers { get; set; }

        // For time-limit validation on server
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? EndedAtUtc { get; set; }
    }

    public class AttemptAnswerItemDto
    {
        [Required] public Guid QuestionId { get; set; }

        /// <summary>Chosen option letter(s), e.g. "A" or "A,C"</summary>
        [Required]
        public string ChosenOption { get; set; }
    }

    public class AttemptFeedbackItemDto
    {
        public Guid QuestionId { get; set; }
        public string CorrectOption { get; set; }
        public string? YourOption { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class AttemptResultDto
    {
        public Guid AttemptAnswerId { get; set; }
        public Guid QuizId { get; set; }
        public int TotalMark { get; set; }
        public double Percentage { get; set; }
        public bool IsPendingEvaluation { get; set; }
        public List<AttemptFeedbackItemDto> Feedback { get; set; }

        /// <summary>
        /// True when a PremiumTaker has used their free attempt and must pay to retry.
        /// The frontend should show the payment option when this is true.
        /// </summary>
        public bool RequiresPaymentForRetry { get; set; }
    }
}

// ── Evaluator submission view ──
public class SubmissionDetailDto
{
    public Guid AttemptAnswerId { get; set; }
    public Guid QuizId { get; set; }
    public string QuizName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int TotalMark { get; set; }
    public int MaxMark { get; set; }
    public double Percentage { get; set; }
    public DateTime SubmittedAt { get; set; }
    public List<SubmissionAnswerDto> Answers { get; set; } = new();
}

public class SubmissionAnswerDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? ChosenOption { get; set; }
    public string CorrectOption { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int MarksAwarded { get; set; }
    public int MaxMarks { get; set; }
}

public class UpdateScoreDto
{
    public int NewTotalMark { get; set; }
    public List<QuestionScoreDto> QuestionScores { get; set; } = new();
}

public class QuestionScoreDto
{
    public Guid QuestionId { get; set; }
    public int MarksAwarded { get; set; }
}
