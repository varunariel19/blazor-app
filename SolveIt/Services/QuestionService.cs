using Google.Protobuf.Collections;
using Microsoft.EntityFrameworkCore;
using Qdrant.Client.Grpc;
using SolveIt.Data;
using SolveIt.Models;

namespace SolveIt.Services;

public class QuestionPagedResult
{
    public List<Question> Questions { get; set; } = [];
    public int? TotalPages { get; set; } 
}

public class QuestionService(IDbContextFactory<AppDbContext> contextFactory , VectorService vectorService , EmbeddingService embeddingService)
{

     private readonly VectorService _vectorService = vectorService;
    private readonly EmbeddingService _embeddingService = embeddingService;


    public async Task<bool> CreateQuestionWithOptionalSolution(
    Question newQuestion,
    string? solutionBody,
    List<string> tagNames)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var normalizedTags = tagNames
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.ToLower().Trim())
                .Distinct()
                .ToList();

            var existingTags = await context.Tags
                .Where(t => normalizedTags.Contains(t.TagName))
                .ToListAsync();

            foreach (var tagName in normalizedTags)
            {
                var tag = existingTags.FirstOrDefault(t => t.TagName == tagName)
                          ?? new Tag { TagName = tagName };

                newQuestion.QuestionTags.Add(tag);
            }

            context.Questions.Add(newQuestion);

            if (!string.IsNullOrWhiteSpace(solutionBody))
            {
                var solution = new Solution
                {
                    Body = solutionBody,
                    Question = newQuestion, 
                    UserId = newQuestion.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                context.Solutions.Add(solution);

                newQuestion.SolutionCount++;

                var user = await context.Users.FindAsync(newQuestion.UserId);
                if (user != null)
                {
                    user.TotalQuestions++;
                    if(!string.IsNullOrWhiteSpace(solutionBody))
                    {
                       user.TotalSolutions++;
                    }
                }
            }

            await context.SaveChangesAsync();

            string content = $"{newQuestion.Title} {newQuestion.Body}";
            var embeddings = await _embeddingService.GetEmbeddingsAsync(content);

            var vectorMapData = new MapField<string, Value>
        {
            { "questionId", new Value { StringValue = newQuestion.QuestionId.ToString() } },
            { "question", new Value { StringValue = newQuestion.Title } }
        };

            await _vectorService.InsertVectorAsync("questions", embeddings!, vectorMapData);

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

    public async Task<QuestionPagedResult> GetAllQuestionsAsync(string? type,int pageNumber,int pageSize, bool includeCount)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var query = context.Questions
            .Include(q => q.User)
            .Include(q => q.QuestionTags)
            .Include(q => q.Solutions)
            .AsQueryable();

        switch (type)
        {
            case "Newest":
                var fromLast24Hours = DateTime.UtcNow.AddHours(-24);

                query = query
                    .Where(q => q.CreatedAt >= fromLast24Hours)
                    .OrderByDescending(q => q.CreatedAt);
                break;

            case "Active":
                query = query.OrderByDescending(q => q.CreatedAt);
                break;

            case "Unanswered":
                query = query
                    .Where(q => q.SolutionCount == 0)
                    .OrderByDescending(q => q.CreatedAt);
                break;

            default:
                query = query.OrderByDescending(q => q.CreatedAt);
                break;
        }

        int? totalPages = null;

        if (includeCount)
        {
            var totalCount = await query.CountAsync();
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        var questions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        

        return new QuestionPagedResult
        {
            Questions = questions,
            TotalPages = totalPages
        };
    }

    public async Task<List<Tag>> GetAllTagsAsync(){

        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Tags
               .ToListAsync();    
       }


    public async Task<QuestionPagedResult> GetQuestionsByTag(string tagType)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var query = context.Questions
            .Where(q => q.QuestionTags.Any(x => x.TagName == tagType));

        var totalCount = await query.CountAsync();

        var questions = await query
            .Include(q => q.QuestionTags)
            .ToListAsync();

        return new QuestionPagedResult
        {
            Questions = questions,
            TotalPages = totalCount
        };
    }



    public async Task<List<Question>> SearchQuestionAsync(string searchedQuery)
    {
        var embeddings = await _embeddingService.GetEmbeddingsAsync(searchedQuery);

        var questionIds = await _vectorService.SearchVectorAsync("questions", embeddings!);

        await using var context = await contextFactory.CreateDbContextAsync();

        var questions = await context.Questions
        .Include(q => q.User)
        .Include(q => q.QuestionTags)
        .Where(q => questionIds.Contains(q.QuestionId))
        .ToListAsync();


        return questionIds
        .Select(id => questions.FirstOrDefault(q => q.QuestionId == id))
        .Where(q => q != null)
        .Take(5)
        .ToList()!;


    }


    public async Task<(List<Question> Questions, int? TotalPages , int? totalQues)>
    GetQuestionsByUserAsync(string userId, int page, int pageSize)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var query = context.Questions
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt);

        var totalCount = await query.CountAsync();

        var questions = await query
            .Include(q => q.QuestionTags)
            .Include(q => q.User)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return (questions, totalPages , totalCount);
    }


    public async Task<List<Question>> GetHotQuestionAsync()
    {
        try
        {
        await using var context = await contextFactory.CreateDbContextAsync();

        var within24Hours = DateTime.UtcNow.AddHours(-24);
        var hotQuestions = await context.Questions.Where(q => q.CreatedAt > within24Hours).OrderByDescending(q => q.VoteCount).ToListAsync();
        return hotQuestions;
             
        }catch(Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }


    public async Task<bool> SaveQuestionAsync(Guid questionId, string userId)
    {
        if ( string.IsNullOrWhiteSpace(userId))
            return false;

        if (!Guid.TryParse(userId, out var parsedUserId))
            return false;

        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            bool alreadySaved = await context.SavedQuestions
                .AnyAsync(sq => sq.QuestionId == questionId && sq.UserId == parsedUserId);

            if (alreadySaved)
                return false;

            var savedQuestion = new SavedQuestion
            {
                QuestionId = questionId,
                UserId = parsedUserId,
            };

            context.SavedQuestions.Add(savedQuestion);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }

    }




    public async Task<List<SavedQuestion>> GetSavedQuestionsAsync(string userId)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            return await context.SavedQuestions
                .Where(sq => sq.UserId == Guid.Parse(userId))
                .Include(sq => sq.Question)
                    .ThenInclude(q => q.QuestionTags)
                .OrderByDescending(sq => sq.SavedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
           
            return [];
        }
    }
}