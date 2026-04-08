namespace SolveIt.Models
{
    public enum VoteType
    {
        Question,
        Solution
    }

    public class Vote
    {
        public Guid VoteId { get; set; }
        public Guid UserId { get; set; }
        public Guid TargetId { get; set; }
        public VoteType TargetType { get; set; }
    }

}