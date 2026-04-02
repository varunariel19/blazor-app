

namespace SolveIt.Models;

public class Comment
{
    public Guid CommentId { get; set; } = Guid.NewGuid();

    public string Body { get; set; } = string.Empty;         
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Guid? QuestionId { get; set; }
    public string UserId { get; set; } = string.Empty;

    public Question? Question { get; set; }
    public User User { get; set; } = null!;
}