using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using SolveIt.Models;

namespace SolveIt.Pages.Account
{
    public class LogoutModel(SignInManager<User> signInManager) : PageModel
    {
        private readonly SignInManager<User> _signInManager = signInManager;

        public async Task<IActionResult> OnGetAsync()
        {
            await _signInManager.SignOutAsync();
            return LocalRedirect("/login");
        }
    }
}
