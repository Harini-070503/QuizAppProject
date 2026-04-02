
using System;
using System.Collections.Generic;       

namespace QuizAppProject.Models
{
    public class Category
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }=string.Empty;
        public DateTime CreatedAt { get; set; }

        public ICollection<Quiz> Quizz { get; set; }
    }

}
