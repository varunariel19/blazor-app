using Microsoft.EntityFrameworkCore;
using SolveIt.Data;
using SolveIt.Models;

namespace SolveIt.Services;

public class QuestionService(IDbContextFactory<AppDbContext> contextFactory)
{
    public async Task<bool> CreateQuestionWithOptionalSolution(Question newQuestion, string? solutionBody, List<string> tagNames)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            foreach (var tagName in tagNames)
            {
                var existingTag = await context.Tags
                    .FirstOrDefaultAsync(t => t.TagName.ToLower() == tagName.ToLower());

                if (existingTag != null)
                    newQuestion.QuestionTags.Add(existingTag);
                else
                    newQuestion.QuestionTags.Add(new Tag { TagName = tagName.ToLower() });
            }

            context.Questions.Add(newQuestion);
            await context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(solutionBody))
            {
                var solution = new Solution
                {
                    Body = solutionBody,
                    QuestionId = newQuestion.QuestionId,
                    UserId = newQuestion.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                context.Solutions.Add(solution);
                newQuestion.SolutionCount = 1;
                await context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<List<Question>> GetAllQuestionsAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Questions
            .Include(q => q.User)
            .Include(q => q.QuestionTags)
            .Include(q => q.Solutions)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }
}