namespace SolveIt.Models;

public class Tag
{
    public Guid TagId { get; set; } = Guid.NewGuid();
    public string TagName { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#6B7280";

    public ICollection<Question> QuestionTags { get; set; } = [];
    public ICollection<Solution> SolutionTags { get; set; } = [];
}