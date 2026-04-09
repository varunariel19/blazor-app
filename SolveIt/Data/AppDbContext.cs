using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SolveIt.Models;

namespace SolveIt.Data

{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<Solution> Solutions => Set<Solution>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Vote> Votes { get; set; }
        public DbSet<SavedQuestion> SavedQuestions { get; set; }
        public DbSet<Conversation> Converstions { get; set; }

        public DbSet<Inbox> Inbox { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Solution --------------------------------------------------
            builder.Entity<Solution>().HasKey(s => s.SolId);

            builder.Entity<Solution>()
                     .Property(q => q.SolId)
                     .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Entity<Solution>()
                .HasMany(s => s.SolutionTags)
                .WithMany(t => t.SolutionTags)
                .UsingEntity(j => j.ToTable("SolutionTags"));


             builder.Entity<Solution>()
                .HasOne(s => s.User)
                .WithMany(u => u.Solutions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Solution>()
            .HasOne(s => s.Question)
            .WithMany(q => q.Solutions)
            .HasForeignKey(s => s.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

            // Solution --------------------------------------------------





            // Question --------------------------------------------
            builder.Entity<Question>()
         .Property(q => q.QuestionId)
         .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Entity<Question>().HasKey(q => q.QuestionId);

            builder.Entity<Question>()
                .HasOne(q => q.User)
                .WithMany(u => u.Questions)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<Question>()
                .HasMany(q => q.QuestionTags)
                .WithMany(t => t.QuestionTags)
                .UsingEntity(j => j.ToTable("QuestionTags"));

            // Question --------------------------------------------





            // Comment -------------------------------------
            builder.Entity<Comment>().HasKey(c => c.CommentId);

            builder.Entity<Comment>()
             .Property(q => q.CommentId)
             .HasDefaultValueSql("NEWSEQUENTIALID()");


            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.Question)
                .WithMany(q => q.Comments)
                .HasForeignKey(c => c.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            // Comment -------------------------------------





            //Tags -----------------------------
            builder.Entity<Tag>().HasKey(t => t.TagId);

            builder.Entity<Tag>()
             .Property(q => q.TagId)
             .HasDefaultValueSql("NEWSEQUENTIALID()");
            //Tags -----------------------------






            //SavedQuestion -----------------------------------
            builder.Entity<SavedQuestion>()
                .HasIndex(sq => new { sq.UserId, sq.QuestionId })
                .IsUnique();
            
            builder.Entity<SavedQuestion>()
                .HasOne(sq => sq.Question)
                .WithMany()
                .HasForeignKey(sq => sq.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
            //SavedQuestion -----------------------------------




            // Conversations -----------------------
            builder.Entity<Conversation>(entity =>
            {
        
                entity.HasKey(e => e.ConversationId);

                entity.Property(e => e.Content)
                      .IsRequired()
                      .HasMaxLength(2000);

              
                entity.HasOne(c => c.Inbox)             
                      .WithMany(i => i.Conversations)
                      .HasForeignKey(c => c.InboxId)
                      .OnDelete(DeleteBehavior.Cascade);


                entity.HasOne(c => c.Sender)
                      .WithMany()
                      .HasForeignKey(c => c.SenderId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.Receiver)
                      .WithMany()
                      .HasForeignKey(c => c.ReceiverId)
                      .OnDelete(DeleteBehavior.NoAction);



                entity.HasIndex(e => new { e.InboxId, e.SendedAt });
            });

            // Conversations -----------------------


            // Inboxes -------------------------------
            builder.Entity<Inbox>(entity =>
            {

                entity.HasKey(e => e.InboxId);
                entity.Property(e => e.InboxId)
                    .HasDefaultValueSql("NEWID()");

                entity.HasOne(c => c.User1)
                    .WithMany(u => u.InboxesAsUser1) 
                    .HasForeignKey(c => c.User1Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.User2)
                    .WithMany(u => u.InboxesAsUser2)
                    .HasForeignKey(c => c.User2Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.User1Id);
                entity.HasIndex(e => e.User2Id);
                entity.HasIndex(e => new { e.User1Id, e.User2Id, e.SendedAt });
            });

            // Inboxes -------------------------------



        }
    }
    }







