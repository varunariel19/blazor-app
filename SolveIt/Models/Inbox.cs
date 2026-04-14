using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolveIt.Models
{
    public class Inbox
    {

        [Key]
        public Guid InboxId { get; set; }
        
        [Required]
        public required string User1Id { get; set; }

        [Required]
        public required string User2Id { get; set; }

        public string LastMsgUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(User1Id))]
        public User User1 { get; set; } = null!;

        [ForeignKey(nameof(User2Id))]
        public User User2 { get; set; } = null!;

        public ICollection<Conversation> Conversations { get; set; } = [];


        [Required]
        [MaxLength(2000)]
        public string RecentText { get; set; } = string.Empty;
        public DateTime SendedAt { get; set; } = DateTime.UtcNow;


    }
}
