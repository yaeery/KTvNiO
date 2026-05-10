using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace KTvNiO.Models
{
    public class Course
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        public string Content { get; set; } // Учебный материал
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public bool IsPublished { get; set; } = true;
        
        // Дополнительные свойства
        public int? DurationHours { get; set; }
        
        // Навигационные свойства
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
        
        public Course()
        {
            Enrollments = new List<Enrollment>();
            Tests = new List<Test>();
        }
    }
}
