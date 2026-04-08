using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SolveIt.Models;

namespace SolveIt.Services;

public class AuthService(UserManager<User> userManager , AuthenticationStateProvider authProvider)
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly  AuthenticationStateProvider _authProvider = authProvider;


    private static readonly string[] Avatars =
    [
        "https://api.dicebear.com/7.x/adventurer/svg?seed=Felix",
        "https://api.dicebear.com/7.x/adventurer/svg?seed=Aneka",
        "https://api.dicebear.com/7.x/adventurer/svg?seed=Zara",
        "https://api.dicebear.com/7.x/adventurer/svg?seed=Milo",
        "https://api.dicebear.com/7.x/adventurer/svg?seed=Nova",
        "https://api.dicebear.com/7.x/adventurer/svg?seed=Axel",
        "https://api.dicebear.com/7.x/adventurer/svg?seed=Luna",
        "https://api.dicebear.com/7.x/adventurer/svg?seed=Blaze",
        "https://api.dicebear.com/7.x/adventurer/svg?seed=Echo",
        "https://api.dicebear.com/7.x/adventurer/svg?seed=Orion",
    ];

    public async Task<IdentityResult> RegisterAsync(string email, string password, string username)
    {
        var displayName = email.Split('@')[0];
        var normalized = (email + username).ToLowerInvariant();
        var hash = normalized.Aggregate(0, (acc, ch) => acc * 31 + ch);
        var index = Math.Abs(hash) % Avatars.Length;
        var avatar = Avatars[index];

        var user = new User
        {
            UserName = email,
            Email = email,
            DisplayName = username,
            AvatarUrl = avatar,
            JoinedAt = DateTime.UtcNow
        };

        return await _userManager.CreateAsync(user, password);
    }


    public async Task<User?> GetCurrentUserAsync()
    {
        var authState = await _authProvider.GetAuthenticationStateAsync();
        var claims = authState.User;

        if (claims.Identity?.IsAuthenticated != true)
            return null;

        var email = claims.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            return null;

        return await _userManager.FindByEmailAsync(email);
    }
}