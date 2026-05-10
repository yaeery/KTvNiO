using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTvNiO.Models
{
    public class Test
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        // Навигационное свойство для вопросов теста
        public ICollection<TestQuestion> Questions { get; set; }

        // Навигационное свойство для результатов тестов
        public ICollection<TestResult> Results { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Test()
        {
            Questions = new HashSet<TestQuestion>();
            Results = new HashSet<TestResult>();
        }
    }
}
