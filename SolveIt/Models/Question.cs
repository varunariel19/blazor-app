
namespace SolveIt.Models

{
    public class Question
    {
        public Guid QuestionId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;        
        public string? CoverImageUrl { get; set; }              
        public int VoteCount { get; set; } = 0;                 
        public int CommentCount { get; set; } = 0;             
        public int SolutionCount { get; set; } = 0;            
        public int ViewCount { get; set; } = 0;                  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        public ICollection<Solution> Solutions { get; set; } = new List<Solution>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Tag> QuestionTags { get; set; } = new List<Tag>();
    }
}