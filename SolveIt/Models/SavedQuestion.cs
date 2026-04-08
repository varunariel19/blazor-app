using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolveIt.Models
{
    public class SavedQuestion
    {
        [Key]
        public Guid  Id { get; set; }
        public  required  Guid UserId { get; set; }
        public  required Guid QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public Question Question { get; set; } = null!;
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}
