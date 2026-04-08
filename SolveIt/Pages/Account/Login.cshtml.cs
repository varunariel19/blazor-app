using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SolveIt.Models;
using SolveIt.UI_state;

namespace SolveIt.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;   
    private readonly UiStateService _uiState;                

    public LoginModel(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        UiStateService uiState)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _uiState = uiState;
    }

    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string? Error { get; set; }

    public async Task<IActionResult> OnPostAsync(string returnUrl = "/")
    {
        var result = await _signInManager.PasswordSignInAsync(
            Email, Password, true, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(Email);

            if (user != null)
                _uiState.HandleUserLogin(user);  

            return LocalRedirect(returnUrl);
        }

        Error = "Invalid email or password.";
        return Page();
    }
}