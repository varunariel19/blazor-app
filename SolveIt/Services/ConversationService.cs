using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SolveIt.Data;
using SolveIt.Hubs;
using SolveIt.Models;
using static SolveIt.Components.Modules.InboxDilog;

namespace SolveIt.Services
{


    public class ConversationService(IDbContextFactory<AppDbContext> contextFactory , IHubContext<ChatHub> hubContext)
    {

        public record ConversationDto(
             Guid ConversationId,
             Guid InboxId,
             string SenderId,
             string ReceiverId,
             string Content,
             DateTime SendedAt,
             bool Seen,
             UserSummaryDto Sender,
             UserSummaryDto Receiver
);



        // send message 


        // load all converstations 

        public async Task SendMessageAsync(ConversationDto newMessage)
        {
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync();

                var inbox = await context.Inbox.FirstOrDefaultAsync(i => i.InboxId == newMessage.InboxId);

                if (inbox != null)
                {
                    inbox.RecentText = newMessage.Content;
                    inbox.SendedAt = newMessage.SendedAt;
                    context.Inbox.Update(inbox);
                }

                var conversation = new Conversation
                {
                    ConversationId = newMessage.ConversationId,
                    InboxId = newMessage.InboxId,
                    SenderId = newMessage.SenderId,
                    ReceiverId = newMessage.ReceiverId,
                    Content = newMessage.Content,
                    SendedAt = newMessage.SendedAt,
                    Seen = false
                };

                context.Converstions.Add(conversation);

                // broadcast this newMessage in that Inbox  
                await hubContext.Clients.Group(newMessage.InboxId.ToString())
                    .SendAsync("ReceiveMessage", newMessage);

                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception($"[SendMessageAsync] Failed to send message: {ex.Message}", ex);
            }
        }

        public record ConvoResult(Guid InboxId, List<Conversation> Conversations);

        public async Task<ConvoResult?> CheckExistingConvo(string myUserId, string selectedUserId)
        {
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync();

                var existingInbox = await context.Inbox
                    .FirstOrDefaultAsync(i =>
                        (i.User1Id == myUserId && i.User2Id == selectedUserId) ||
                        (i.User1Id == selectedUserId && i.User2Id == myUserId));

                if (existingInbox != null)
                {
                    List<Conversation> existingChats =
                        await RetreiveAllConversationsAsync(existingInbox.InboxId);

                    return new ConvoResult(existingInbox.InboxId, existingChats);
                }

                var newInbox = new Inbox
                {
                    InboxId = Guid.NewGuid(),
                    User1Id = myUserId,
                    User2Id = selectedUserId,
                    RecentText = string.Empty,
                    SendedAt = DateTime.UtcNow
                };

                context.Inbox.Add(newInbox);
                await context.SaveChangesAsync();

                return new ConvoResult(newInbox.InboxId, []);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[CheckExistingConvo] {ex.Message}");
                return null;
            }
        }




        private async Task<List<Conversation>> RetreiveAllConversationsAsync(Guid InboxId)
        {
            List<Conversation> existingChats = [];

            try
            {
                if (InboxId == Guid.Empty) return existingChats;

                await using var context = await contextFactory.CreateDbContextAsync();
                existingChats = await context.Converstions
                    .Where(c => c.InboxId == InboxId)
                    .Include(c => c.Sender)    
                    .Include(c => c.Receiver)  
                    .OrderBy(c => c.SendedAt)  
                    .ToListAsync();

                return existingChats;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[RetreiveAllConversationsAsync] {ex.Message}");
                return existingChats;
            }
        }


    }
}
