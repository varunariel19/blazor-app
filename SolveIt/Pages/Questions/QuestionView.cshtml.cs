using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SolveIt.Data;
using SolveIt.Models;
using System.Security.Claims;

namespace SolveIt.Pages.Questions
{
    public class QuestionViewModel : PageModel
    {
        private readonly AppDbContext _db;

        public QuestionViewModel(AppDbContext db)
        {
            _db = db;
        }

        public Question Question { get; set; } = null!;
        public string? CurrentUserId { get; set; }
        public bool IsLoggedIn { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid questionId)
        {
            CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IsLoggedIn = !string.IsNullOrEmpty(CurrentUserId);

            var question = await _db.Questions
                .Include(q => q.User)
                .Include(q => q.QuestionTags)
                .Include(q => q.Solutions)
                    .ThenInclude(s => s.User)
                .Include(q => q.Solutions)
                    .ThenInclude(s => s.SolutionTags)
                .Include(q => q.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

            if (question == null)
                return NotFound();

            question.ViewCount++;
            await _db.SaveChangesAsync();

            Question = question;
            return Page();
        }
    
    

    
    }
}
