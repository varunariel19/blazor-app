namespace SolveIt.Services
{
    public class UserStateService
    {
        public UserDto? CurrentUser { get; private set; }
        public bool IsAuthenticated => CurrentUser != null;
        public event Action? OnChange;

        public void SetUser(UserDto user)
        {
            CurrentUser = user;
            OnChange?.Invoke();
        }

        public void Clear()
        {
            CurrentUser = null;
            OnChange?.Invoke();
        }
    }

    public class UserDto
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string DisplayName { get; set; } = "";

        public string AvatarUrl { get; set; } = "";
    }
}
