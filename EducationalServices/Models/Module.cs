using EducationalServices.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace EducationalServices.Models
{
    public class Module
    {
        [Key]
        public int ModuleId { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        [Required]
        [StringLength(20)]
        public string SubjectCode { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Range(1, 1000)]
        public int DurationInHours { get; set; }

        [StringLength(20)]
        public string Difficulty { get; set; }

        [Range(0, 10000)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public virtual ICollection<TutorModule> TutorModules { get; set; }

        public virtual ICollection<Quiz> Quizzes { get; set; }
    }

    public class TutorModule
    {
        [Key]
        public int TutorModuleId { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [ForeignKey("ModuleId")]
        public virtual Module Module { get; set; }

        [Required]
        public string TutorId { get; set; }

        [ForeignKey("TutorId")]
        public virtual ApplicationUser User { get; set; }
    }


    public class StudentModule
    {
        [Key]
        public int StudentModuleId { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [ForeignKey("ModuleId")]
        public virtual Module Module { get; set; }

        [Required]
        public string StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual ApplicationUser User { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [Range(0, 10000)]
        [DataType(DataType.Currency)]
        public decimal TotalPaid { get; set; }
    }


    public class Quiz
    {
        [Key]
        public int QuizId { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [ForeignKey("ModuleId")]
        public virtual Module Module { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        [Required]
        [Range(1, 100)]
        public int MaxAttempts { get; set; }

        [Range(0, 1000)]
        public int TimeLimit { get; set; }

        public virtual ICollection<Question> Questions { get; set; }

        public virtual ICollection<QuizAttempt> Attempts { get; set; }

        public bool IsCompleted { get; set; }

        public virtual ICollection<QuizRating> QuizRatings { get; set; }

        public Quiz()
        {
            QuizRatings = new HashSet<QuizRating>();
        }
    }


    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Text { get; set; }

        [StringLength(2000)]
        public string Options { get; set; }

        [Required]
        [StringLength(1000)]
        public string CorrectAnswer { get; set; }

        [Range(0, 100)]
        public int Marks { get; set; }

        [Required]
        public QuestionType QuestionType { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }
    }






    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse,
        FillInTheBlank,
        ShortAnswer,
        Essay,
        Matching,
        Ranking,
        MultipleAnswers
    }

    public class QuizAttempt
    {
        [Key]
        public int AttemptId { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }

        [Required]
        public string StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual ApplicationUser Student { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:g}", ApplyFormatInEditMode = true)]
        public DateTime StartTime { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:g}", ApplyFormatInEditMode = true)]
        public DateTime? EndTime { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Score must be a non-negative number.")]
        public int Score { get; set; }

        public bool IsCompleted { get; set; }

        // One-to-many relationship with StudentAnswer
        public virtual ICollection<StudentAnswer> Answers { get; set; }

        // Calculated property to show the quiz duration
        [NotMapped]
        public TimeSpan? Duration
        {
            get
            {
                if (!EndTime.HasValue)
                {
                    return null;
                }
                return EndTime.Value - StartTime;
            }
        }

        // Calculated property to sum the total marks from answers
        [NotMapped]
        public int TotalMarks => Answers?.Sum(a => a.Question.Marks) ?? 0;

        // Assuming each attempt has only one QuizRating (if many-to-one)
        public virtual QuizRating QuizRating { get; set; }

        // Optional if you allow multiple ratings for an attempt
        public virtual ICollection<QuizRating> QuizRatings { get; set; }

        public QuizAttempt()
        {
            Answers = new HashSet<StudentAnswer>();
            QuizRatings = new HashSet<QuizRating>();
        }
    }


    public class StudentAnswer
    {
        [Key]
        public int AnswerId { get; set; }

        [Required]
        public int QuizAttemptId { get; set; }

        [ForeignKey("QuizAttemptId")]
        public virtual QuizAttempt QuizAttempt { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }

        [Required]
        [StringLength(2000)]
        public string Answer { get; set; }

        [Range(0, int.MaxValue)]
        public int MarksObtained { get; set; }
    }
}
