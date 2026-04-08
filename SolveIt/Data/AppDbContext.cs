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


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Question>().HasKey(q => q.QuestionId);
            builder.Entity<Solution>().HasKey(s => s.SolId);
            builder.Entity<Comment>().HasKey(c => c.CommentId);
            builder.Entity<Tag>().HasKey(t => t.TagId);

            builder.Entity<Question>()
                 .Property(q => q.QuestionId)
                 .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Entity<Solution>()
             .Property(q => q.SolId)
             .HasDefaultValueSql("NEWSEQUENTIALID()");


            builder.Entity<Comment>()
             .Property(q => q.CommentId)
             .HasDefaultValueSql("NEWSEQUENTIALID()");


            builder.Entity<Tag>()
             .Property(q => q.TagId)
             .HasDefaultValueSql("NEWSEQUENTIALID()");



            builder.Entity<Question>()
                .HasOne(q => q.User)
                .WithMany(u => u.Questions)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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

            builder.Entity<Question>()
                .HasMany(q => q.QuestionTags)
                .WithMany(t => t.QuestionTags)
                .UsingEntity(j => j.ToTable("QuestionTags"));

            builder.Entity<Solution>()
                .HasMany(s => s.SolutionTags)
                .WithMany(t => t.SolutionTags)
                .UsingEntity(j => j.ToTable("SolutionTags"));
        }
    }
}