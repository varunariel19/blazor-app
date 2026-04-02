
namespace SolveIt.Models;

public class Solution
{
    public Guid SolId { get; set; } = Guid.NewGuid();

    public string Body { get; set; } = string.Empty;        

    public int Likes { get; set; } = 0;
    public int Dislikes { get; set; } = 0;
    public int Votes { get; set; } = 0; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Guid QuestionId { get; set; }
    public string UserId { get; set; } = string.Empty;

    public Question Question { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<Tag> SolutionTags { get; set; } = new List<Tag>();

}