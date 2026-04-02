using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolveIt.Models;

namespace SolveIt.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<User> _signInManager;

    public LoginModel(SignInManager<User> signInManager)
        => _signInManager = signInManager;

    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string? Error { get; set; }

    public async Task<IActionResult> OnPostAsync(string returnUrl = "/")
    {
        var result = await _signInManager.PasswordSignInAsync(
            Email, Password, true, lockoutOnFailure: false);

        if (result.Succeeded)
            return LocalRedirect(returnUrl);

        Error = "Invalid email or password.";
        return Page(); 
    }
}
