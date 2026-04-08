using Microsoft.EntityFrameworkCore;
using SolveIt.Data;
using SolveIt.Models;

namespace SolveIt.Services
{
    public class QuestionInteractionService(IDbContextFactory<AppDbContext> contextFactory)
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

        public async Task<QuestionVoteResult> VoteOnQuestionAsync(Guid questionId, Guid userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var question = await context.Questions
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

            if (question == null)
                throw new KeyNotFoundException("Question not found.");

            var existingVote = await context.Votes
                .FirstOrDefaultAsync(v =>
                    v.UserId == userId &&
                    v.TargetId == questionId &&
                    v.TargetType == VoteType.Question);

            if (existingVote != null)
            {
                return new QuestionVoteResult
                {
                    VoteCount = question.VoteCount,
                    Message = "Already UpVoted"
                };
            }
            else
            {
                var vote = new Vote
                {
                    VoteId = Guid.NewGuid(),
                    UserId = userId,
                    TargetId = questionId,
                    TargetType = VoteType.Question
                };

                context.Votes.Add(vote);
                question.VoteCount++;
            }

            question.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return new QuestionVoteResult
            {
                VoteCount = question.VoteCount,
                Message = "UpVoted"
                
            };
        }


        public async Task<QuestionVoteResult> VoteOnSolutionAsync(Guid questionId, Guid solutionId, Guid userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var currentSol = await context.Solutions
                .FirstOrDefaultAsync(s => s.QuestionId == questionId && s.SolId == solutionId);

            if (currentSol == null)
                throw new KeyNotFoundException("Solution not found.");

            var existingVote = await context.Votes
                .FirstOrDefaultAsync(v =>
                    v.UserId == userId &&
                    v.TargetId == solutionId &&
                    v.TargetType == VoteType.Solution);

            if (existingVote != null)
            {
                return new QuestionVoteResult
                {
                    VoteCount = currentSol.Votes,
                    Message = "You have already upvoted this solution."
                };
            }

            var vote = new Vote
            {
                VoteId = Guid.NewGuid(),
                UserId = userId,
                TargetId = solutionId,      
                TargetType = VoteType.Solution, 
            };

            context.Votes.Add(vote);

            currentSol.Votes++;

            await context.SaveChangesAsync();

            return new QuestionVoteResult
            {
                VoteCount = currentSol.Votes,
                Message = "Solution Upvoted!"
            };
        }

        public async Task<CommentResult> AddCommentAsync(Guid questionId, string userId, string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("Comment body cannot be empty.");

            if (body.Length > 1000)
                throw new ArgumentException("Comment cannot exceed 1000 characters.");

            await using var context = await _contextFactory.CreateDbContextAsync();

            var question = await context.Questions
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

            if (question == null)
                throw new KeyNotFoundException("Question not found.");

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var comment = new Comment
            {
                CommentId = Guid.NewGuid(),
                Body = body.Trim(),
                QuestionId = questionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            question.CommentCount++;
            question.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return new CommentResult
            {
                CommentId = comment.CommentId,
                Body = comment.Body,
                CreatedAt = comment.CreatedAt,
                UserId = userId,
                UserName = user.DisplayName  
            };
        }

       
        public async Task<SolutionVoteResult> LikeSolutionAsync(Guid solId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var solution = await context.Solutions
                .FirstOrDefaultAsync(s => s.SolId == solId);

            if (solution == null)
                throw new KeyNotFoundException("Solution not found.");

            solution.Likes++;
            solution.Votes = solution.Likes - solution.Dislikes;
            solution.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return new SolutionVoteResult
            {
                Likes = solution.Likes,
                Dislikes = solution.Dislikes,
                Votes = solution.Votes
            };
        }

       
        public async Task<SolutionVoteResult> DislikeSolutionAsync(Guid solId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var solution = await context.Solutions
                .FirstOrDefaultAsync(s => s.SolId == solId);

            if (solution == null)
                throw new KeyNotFoundException("Solution not found.");

            solution.Dislikes++;
            solution.Votes = solution.Likes - solution.Dislikes;
            solution.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return new SolutionVoteResult
            {
                Likes = solution.Likes,
                Dislikes = solution.Dislikes,
                Votes = solution.Votes
            };
        }


        public async Task<object> AddSolutionAsync(Guid questionId, string userId, string body)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var question = await context.Questions
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

            if (question == null)
                throw new KeyNotFoundException("Question not found.");

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new ArgumentException("User not found.");

            var solution = new Solution
            {
                SolId = Guid.NewGuid(),
                Body = body,
                QuestionId = questionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            context.Solutions.Add(solution);

            question.SolutionCount += 1;

            await context.SaveChangesAsync();

            return new
            {
                solId = solution.SolId,
                body = solution.Body,
                userName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName,
                avatarUrl = user.AvatarUrl ?? string.Empty,
                createdAt = solution.CreatedAt
            };
        }

    }



    public class QuestionVoteResult
    {
        public int VoteCount { get; set; }
        public string Message { get; set; }
    }

    public class CommentResult
    {
        public Guid CommentId { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    public class SolutionVoteResult
    {
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public int Votes { get; set; }
    }
}
