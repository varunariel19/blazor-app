
using System.ComponentModel.DataAnnotations;

namespace SolveIt.Models

{
    public class Question
    {
        public Guid QuestionId { get; set; }

        [Required]
        public required string Title { get; set; }
        public string Body { get; set; } = string.Empty;        
        public string? CoverImageUrl { get; set; }              
        public int VoteCount { get; set; }                 
        public int CommentCount { get; set; }         
        public int SolutionCount { get; set; }            
        public int ViewCount { get; set; }                  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public required string UserId { get; set; }
        public User User { get; set; } = null!;

        public ICollection<Solution> Solutions { get; set; } = [];
        public ICollection<Comment> Comments { get; set; } = [];
        public ICollection<Tag> QuestionTags { get; set; } = [];

    }
}