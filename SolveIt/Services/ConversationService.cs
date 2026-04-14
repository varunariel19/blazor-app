using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SolveIt.Data;
using SolveIt.Hubs;
using SolveIt.Models;

namespace SolveIt.Services
{
    public class ConversationService(
        IDbContextFactory<AppDbContext> contextFactory,
        IHubContext<ChatHub> hubContext)
    {
        private readonly IHubContext<ChatHub> _hubContext = hubContext;
        private const string UserIdPrefix = "user-";

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


        public class UserInboxDetails
        {
            public required string UserId { get; set; }
            public required string Username { get; set; }
            public required string AvatarUrl { get; set; }
        }

        public class OpenedUserChat
        {
            public required string UserId { get; set; }

            public required string Username { get; set; }

            public required string AvatarUrl { get; set; }

            public required Guid InboxId { get; set; }
        }

        public record UserSummaryDto(string AvatarUrl, string DisplayName);

        public class InboxMessage
        {
            public Guid InboxId { get; set; } = Guid.Empty;
            public UserInboxDetails? User { get; set; }
            public string RecentMessage { get; set; } = string.Empty;
            public string LastMsgUserId { get; set; } = string.Empty;
            public bool IsRead { get; set; } = false;
            public DateTime ReceivedAt { get; set; } = DateTime.Now;
        }

        public async Task SendMessageAsync(ConversationDto newMessage)
        {
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync();

                // Update inbox recent text
                var inbox = await context.Inbox
                    .FirstOrDefaultAsync(i => i.InboxId == newMessage.InboxId);

                if (inbox != null)
                {
                    inbox.RecentText = newMessage.Content;
                    inbox.LastMsgUserId = newMessage.SenderId;
                    inbox.SendedAt = newMessage.SendedAt;
                    context.Inbox.Update(inbox);
                }

                // Save conversation
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
                await context.SaveChangesAsync();


                await _hubContext.Clients
                    .Group(UserIdPrefix + newMessage.ReceiverId)
                    .SendAsync("ReceiveMessage", newMessage);

                await _hubContext.Clients
                    .Group(UserIdPrefix + newMessage.SenderId)
                    .SendAsync("ReceiveMessage", newMessage);
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
                    var existingChats = await RetreiveAllConversationsAsync(existingInbox.InboxId);
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

        public async Task<List<Conversation>> RetreiveAllConversationsAsync(Guid inboxId)
        {
            try
            {
                if (inboxId == Guid.Empty) return [];

                await using var context = await contextFactory.CreateDbContextAsync();

                var res = await context.Converstions
                    .Where(c => c.InboxId == inboxId)
                    .Include(c => c.Sender)
                    .Include(c => c.Receiver)
                    .OrderBy(c => c.SendedAt)
                    .ToListAsync();

                return res;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[RetreiveAllConversationsAsync] {ex.Message}");
                return [];
            }
        }


        public async Task<List<InboxMessage>> LoadUserInboxesAsync(string currentUserId)
        {
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync();

                var inboxes = await context.Inbox
                    .Include(i => i.User1)
                    .Include(i => i.User2)
                    .Where(i => (i.User1Id == currentUserId || i.User2Id == currentUserId)
                                && i.Conversations.Any())
                    .ToListAsync();

                var latestMessages = await context.Converstions
                    .Where(c => c.ReceiverId == currentUserId)
                    .GroupBy(c => c.InboxId)
                    .Select(g => g.OrderByDescending(x => x.SendedAt).FirstOrDefault())
                    .ToListAsync();

                var latestDict = latestMessages
                    .Where(x => x != null)
                    .ToDictionary(x => x!.InboxId, x => x);

                var result = inboxes
                    .Select(inbox =>
                    {
                        var otherUser = inbox.User1Id == currentUserId
                            ? inbox.User2
                            : inbox.User1;

                        latestDict.TryGetValue(inbox.InboxId, out var latestMsg);

                        return new InboxMessage
                        {
                            InboxId = inbox.InboxId,
                            ReceivedAt = inbox.SendedAt,
                            RecentMessage = inbox.RecentText,
                            IsRead = latestMsg?.Seen ?? true,
                            User = new UserInboxDetails
                            {
                                UserId = otherUser!.Id,
                                Username = otherUser.DisplayName,
                                AvatarUrl = otherUser.AvatarUrl
                            }
                        };
                    })
                    .OrderByDescending(x => x.ReceivedAt)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[LoadUserInboxesAsync] {ex.Message}");
                return [];
            }
        }

        public async Task ReadMessages(Guid inboxId, string userId)
        {
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync();

                var latestSeenItem = await context.Converstions
                    .Where(c => c.InboxId == inboxId && c.ReceiverId == userId && c.Seen == true)
                    .OrderByDescending(c => c.SendedAt)
                    .FirstOrDefaultAsync();

                if (latestSeenItem == null)
                {
                    await ReadAllMessages(userId, inboxId);
                }
                else
                {
                    DateTime lastSeenMsgTime = latestSeenItem.SendedAt;

                    await context.Converstions
                        .Where(c => c.InboxId == inboxId
                                 && c.ReceiverId == userId
                                 && c.SendedAt > lastSeenMsgTime  
                                 && c.Seen == false)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Seen, true));
                }
            }
            catch (Exception)
            {
                throw; 
            }
        }

        public async Task ReadAllMessages(string userId, Guid? inboxId)
        {
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync();

                var query = context.Converstions
                    .Where(c => c.ReceiverId == userId && c.Seen == false);

                if (inboxId.HasValue && inboxId != Guid.Empty)
                {
                    query = query.Where(c => c.InboxId == inboxId.Value);
                }

                await query.ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Seen, true));
            }
            catch (Exception)
            {
                throw; 
            }
        }


    }
}