using Microsoft.AspNetCore.Identity;

namespace SolveIt.Models;

public class User : IdentityUser // already contains pass and email fields
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }


    public int Reputation { get; set; } = 0;
    public int TotalQuestions { get; set; } = 0;
    public int TotalSolutions { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    public bool IsBanned { get; set; } = false;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSeenAt { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<Solution> Solutions { get; set; } = new List<Solution>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}