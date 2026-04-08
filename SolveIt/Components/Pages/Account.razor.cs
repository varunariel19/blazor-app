using SolveIt.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using SolveIt.UI_state;

namespace SolveIt.Components.Pages
{
    public partial class Account : ComponentBase, IDisposable
    {
        [Inject] protected UiStateService UiState { get; set; } = default!;
        [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Inject] protected UserManager<User> UserManager { get; set; } = default!;
        [Inject] protected NavigationManager NavManager { get; set; } = default!;

        protected User? currentUser;

        protected ProfileForm profileForm = new();
        protected EmailForm emailForm = new();
        protected PasswordForm passwordForm = new();

        public class ProfileForm
        {
            public string DisplayName { get; set; } = "";
            public string Bio { get; set; } = "";
            public string? AvatarUrl { get; set; }
        }

        public class EmailForm
        {
            public string Email { get; set; } = "";
        }

        public class PasswordForm
        {
            public string Current { get; set; } = "";
            public string New { get; set; } = "";
            public string Confirm { get; set; } = "";
        }

        protected override async Task OnInitializedAsync()
        {
            UiState.OnChange += StateHasChanged;

            currentUser = UiState.UserData;

            if (currentUser == null)
            {
                var authState = await AuthProvider.GetAuthenticationStateAsync();
                var userClaims = authState.User;

                if (userClaims.Identity?.IsAuthenticated == true)
                {
                    var email = userClaims.FindFirst(ClaimTypes.Email)?.Value;

                    if (!string.IsNullOrEmpty(email))
                    {
                        var user = await UserManager.FindByEmailAsync(email);

                        if (user != null)
                        {
                            currentUser = user;
                            UiState.HandleUserLogin(user);
                        }
                    }
                }
            }

            if (currentUser != null)
            {
                profileForm.DisplayName = currentUser.DisplayName ?? "";
                profileForm.Bio = currentUser.Bio ?? "";
                profileForm.AvatarUrl = currentUser.AvatarUrl;
                emailForm.Email = currentUser.Email ?? "";
            }
        }

        public void Dispose()
        {
            UiState.OnChange -= StateHasChanged;
        }

        protected void HandleProfileUpdate()
        {
            Console.WriteLine("Profile updated");
        }

        protected void HandleEmailUpdate()
        {
            Console.WriteLine("Email updated");
        }

        protected void HandlePasswordChange()
        {
            Console.WriteLine("Password changed");
        }

        protected void DeleteAccount()
        {
            Console.WriteLine("Account deleted");
        }
    }
}