using Microsoft.EntityFrameworkCore;
using SolveIt.Data;
using SolveIt.Models;

namespace SolveIt.Services
{
    public class ConversationService(IDbContextFactory<AppDbContext> contextFactory)
    {



        // send message 


        // load all converstations 

        public async Task SendMessageAsync(Guid convoId , Guid )
        {
             try
            {
                await using var context = await contextFactory.CreateDbContextAsync();
               

                // first create inbox of that two user if not exists 


                // 



            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public async Task<Inbox?> CheckExistingConvo(string myUserId, string selectedUserId)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            var existingInbox = await context.Inbox
                .FirstOrDefaultAsync(i =>
                    (i.User1Id == myUserId && i.User2Id == selectedUserId) ||
                    (i.User1Id == selectedUserId && i.User2Id == myUserId));

            if (existingInbox != null)
            {
                return existingInbox;
            }

            return null;
        }


    }
}
