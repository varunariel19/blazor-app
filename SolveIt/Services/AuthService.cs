using Microsoft.AspNetCore.Identity;
using SolveIt.Models;

namespace SolveIt.Services;

public class AuthService(UserManager<User> userManager)
{
    private readonly UserManager<User> _userManager = userManager;

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
        var avatar = Avatars[Random.Shared.Next(Avatars.Length)];

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
}