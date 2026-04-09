using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolveIt.Models
{
    public class Conversation
    {

        [Key]
         public Guid ConversationId { get; set; }   
 
        [Required]
        public required Guid InboxId { get; set; }  

        [Required]
        public required string SenderId { get; set; }

        [Required]
        public required string ReceiverId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime SendedAt { get; set; } = DateTime.UtcNow;


        public bool Seen { get; set; } = false;
        
        [ForeignKey(nameof(SenderId))]
        public User Sender { get; set; } = null!;

        [ForeignKey(nameof(ReceiverId))]
        public User Receiver { get; set; } = null!;

        [ForeignKey(nameof(InboxId))]
        public Inbox Inbox { get; set; } = null!;

       
        public void MarkAsSeen(string requestingUserId)
        {
            if (requestingUserId == SenderId)
                throw new InvalidOperationException("Sender cannot mark their own message as seen.");

            if (requestingUserId != ReceiverId)
                throw new InvalidOperationException("Only the receiver can mark this message as seen.");

            Seen = true;
        }
    }
}