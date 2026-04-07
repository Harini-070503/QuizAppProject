using Microsoft.EntityFrameworkCore;
using QuizAppProject.Models;

namespace QuizAppProject.Context
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<UserDetails> UserDetails => Set<UserDetails>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Quiz> Quizzes => Set<Quiz>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<Option> Options => Set<Option>();
        public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();
        public DbSet<AttemptAnswerDetail> AttemptAnswerDetails => Set<AttemptAnswerDetail>();
        public DbSet<QuizGroup> QuizGroups => Set<QuizGroup>();
        public DbSet<QuizGroupMember> QuizGroupMembers => Set<QuizGroupMember>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<QuizAllocation> QuizAllocations => Set<QuizAllocation>();
        public DbSet<QuizPayment> QuizPayments => Set<QuizPayment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // ---------- User ----------
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(x => x.UserId);
                e.Property(x => x.UserId)
                    .HasDefaultValueSql("NEWID()");

                e.Property(x => x.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                e.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(254);

                e.HasIndex(x => x.Username).IsUnique();
                e.HasIndex(x => x.Email).IsUnique();

                e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
                e.Property(x => x.Salt).IsRequired().HasMaxLength(128);

                e.Property(x => x.Role)
                    .IsRequired()
                    .HasMaxLength(20);

                e.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                e.HasOne(x => x.UserDetails)

                    .WithOne(x => x.User)
                    .HasForeignKey<UserDetails>(ud => ud.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(x => x.Quizzes)
                    .WithOne(q => q.User)
                    .HasForeignKey(q => q.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- UserDetails ----------
            modelBuilder.Entity<UserDetails>(e =>
            {

                e.ToTable("UserDetails");
                e.HasKey(x => x.UserDetailsId);
                e.Property(x => x.UserDetailsId)
                    .HasDefaultValueSql("NEWID()");
                e.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.Name).HasMaxLength(100);
                e.Property(x => x.PhoneNumber).HasMaxLength(20);
                e.Property(x => x.AddressLine1).HasMaxLength(200);
                e.Property(x => x.AddressLine2).HasMaxLength(200);
                e.Property(x => x.State).HasMaxLength(100);
                e.Property(x => x.City).HasMaxLength(100);
                e.Property(x => x.Pincode).HasMaxLength(20);
            });

            // ---------- Category ----------
            modelBuilder.Entity<Category>(e =>
            {
                e.ToTable("Categories");
                e.HasKey(x => x.CategoryId);
                e.Property(x => x.CategoryId)
                    .HasDefaultValueSql("NEWID()");
                e.Property(x => x.CategoryName)
                    .IsRequired()
                    .HasMaxLength(100);
                e.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                e.HasMany(x => x.Quizz)
                    .WithOne(q => q.Category)
                    .HasForeignKey(q => q.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ---------- Quiz ----------
            modelBuilder.Entity<Quiz>(e =>
            {
                e.ToTable("Quizzes");
                e.HasKey(x => x.QuizId);
                e.Property(x => x.QuizId)
                    .HasDefaultValueSql("NEWID()");
                e.Property(x => x.QuizName)
                    .IsRequired()
                    .HasMaxLength(150);

                e.Property(x => x.Description)
                                  .HasMaxLength(1000);
                e.Property(x => x.DifficultyLevel)
                    .HasMaxLength(30);
                e.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                e.HasMany(x => x.Questions)
                  .WithOne(q => q.Quiz)
                  .HasForeignKey(q => q.QuizId)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------- Question ----------
            modelBuilder.Entity<Question>(e =>

            {
                e.ToTable("Questions");
                e.HasKey(x => x.QuestionId);
                e.Property(x => x.QuestionId)
                    .HasDefaultValueSql("NEWID()");
                e.Property(x => x.QuestionText)
                    .IsRequired();
                e.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Implemented as 1:1 -> one 'Options' row per question containing A-D
                e.HasOne(q => q.Options)
                 .WithOne(o => o.Question)
                 .HasForeignKey<Option>(o => o.QuestionId)
                 .OnDelete(DeleteBehavior.Cascade);

            });

            // ---------- Option ----------
            modelBuilder.Entity<Option>(e =>
            {
                e.ToTable("Options");
                e.HasKey(x => x.OptionId);
                e.Property(x => x.OptionId)
                    .HasDefaultValueSql("NEWID()");
                e.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                e.Property(x => x.OptionA).IsRequired();
                e.Property(x => x.OptionB).IsRequired();
                e.Property(x => x.OptionC).IsRequired(false);
                e.Property(x => x.OptionD).IsRequired(false);

                e.Property(x => x.CorrectOption)
                                    .IsRequired()
                                    .HasMaxLength(20); // e.g. "A" or "A,C,D"
            });

            // ---------- AttemptAnswer ----------
            modelBuilder.Entity<AttemptAnswer>(e =>
            {
                e.ToTable("AttemptAnswers");
                e.HasKey(x => x.AttemptAnswerId);
                e.Property(x => x.AttemptAnswerId)
                    .HasDefaultValueSql("NEWID()");
                e.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                e.HasOne(x => x.User)

.WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Quiz)
                    .WithMany()
                    .HasForeignKey(x => x.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasMany(x => x.Details)
                    .WithOne(d => d.AttemptAnswer)
                    .HasForeignKey(d => d.AttemptAnswerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------- AttemptAnswerDetail ----------
            modelBuilder.Entity<AttemptAnswerDetail>(e =>
            {
                e.ToTable("AttemptAnswerDetails");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
                e.Property(x => x.ChosenOption).HasMaxLength(20).IsRequired(false);
                e.HasOne(x => x.Question)
                    .WithMany()
                    .HasForeignKey(x => x.QuestionId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ---------- QuizGroup ----------
            modelBuilder.Entity<QuizGroup>(e =>
            {
                e.ToTable("QuizGroups");
                e.HasKey(x => x.GroupId);
                e.Property(x => x.GroupId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.GroupName).IsRequired().HasMaxLength(100);
                e.Property(x => x.Description).HasMaxLength(500);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                e.HasOne(x => x.Evaluator)
                    .WithMany()
                    .HasForeignKey(x => x.EvaluatorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- QuizGroupMember ----------
            modelBuilder.Entity<QuizGroupMember>(e =>
            {
                e.ToTable("QuizGroupMembers");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasDefaultValueSql("NEWID()");
                e.Property(x => x.AddedAt).HasDefaultValueSql("GETUTCDATE()");
                e.HasIndex(x => new { x.GroupId, x.UserId }).IsUnique();
                e.HasOne(x => x.Group)
                    .WithMany(g => g.Members)
                    .HasForeignKey(x => x.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------- Quiz.GroupId ----------
            modelBuilder.Entity<Quiz>(e =>
            {
                e.HasOne(x => x.Group)
                    .WithMany(g => g.Quizzes)
                    .HasForeignKey(x => x.GroupId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ---------- QuizPayment ----------
            modelBuilder.Entity<QuizPayment>(e =>
            {
                e.ToTable("QuizPayments");
                e.HasKey(x => x.PaymentId);
                e.Property(x => x.PaymentId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.Amount).HasColumnType("decimal(10,2)");
                e.Property(x => x.SubscriptionType).IsRequired().HasMaxLength(20);
                e.Property(x => x.Status).IsRequired().HasMaxLength(20);
                e.Property(x => x.TransactionRef).HasMaxLength(200).IsRequired(false);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Quiz)
                    .WithMany()
                    .HasForeignKey(x => x.QuizId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ---------- QuizAllocation ----------
            modelBuilder.Entity<QuizAllocation>(e =>
            {
                e.ToTable("QuizAllocations");
                e.HasKey(x => x.AllocationId);
                e.Property(x => x.AllocationId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.AllocatedAt).HasDefaultValueSql("GETUTCDATE()");
                e.HasIndex(x => new { x.QuizId, x.UserId }).IsUnique();
                e.HasOne(x => x.Quiz)
                    .WithMany()
                    .HasForeignKey(x => x.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------- Notification ----------
            modelBuilder.Entity<Notification>(e =>
            {
                e.ToTable("Notifications");
                e.HasKey(x => x.NotificationId);
                e.Property(x => x.NotificationId).HasDefaultValueSql("NEWID()");
                e.Property(x => x.Type).IsRequired().HasMaxLength(50);
                e.Property(x => x.Title).IsRequired().HasMaxLength(200);
                e.Property(x => x.Message).IsRequired().HasMaxLength(500);
                e.Property(x => x.LinkUrl).HasMaxLength(300).IsRequired(false);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

