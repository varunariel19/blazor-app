using SolveIt.Models;


namespace SolveIt.UI_state
{
    public class UiStateService
    {
        #region Variables 
        public List<Question> Questions => _questions;
        private List<Question> _questions = [];
        public bool IsLoggedIn = false;
        private User? _user = null;
        public User? UserData => _user;
        public event Action? OnChange;
        public event Func<Task>? OnUserLoaded;
        private bool _showAskForm;
        #endregion

        #region Functions
        public bool ShowAskForm
        {
            get => _showAskForm;
            private set
            {
                _showAskForm = value;
                Notify();
            }
        }

        #region Dilog Box [ASK QUESTION]
        public void ToggleAskForm() => ShowAskForm = !ShowAskForm;
        public void HideAskForm() => ShowAskForm = false;
        #endregion

        #region Questions Related 
        public void SetQuestions(List<Question> questions)
        {
            _questions = questions;
            Notify();
        }
        public void AddQuestion(Question question)
        {
            _questions.Insert(0, question);
            Notify();
        }
        public void ClearQuestions()
        {
            _questions.Clear();
            Notify();
        }
        #endregion

       
        public void PresistUserState(User user)
        {
            _user = user;
            Notify();
        }

    
        public void DestroyUserState()
        {
            _user = null;
            Notify();
        }

        public void HandleUserLogin(User user)
        {
            IsLoggedIn = true;
            _user = user;  // Set user directly WITHOUT calling Notify()
            NotifyUserLoaded();  // Only fire the user loaded event, don't trigger OnChange yet
        }

        #endregion

        private void Notify() => OnChange?.Invoke();

        private async void NotifyUserLoaded()
        {
            if (OnUserLoaded is not null)
            {
                var handlers = OnUserLoaded.GetInvocationList().Cast<Func<Task>>().ToList();
                foreach (var handler in handlers)
                {
                    try
                    {
                        await handler.Invoke();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in OnUserLoaded handler: {ex.Message}");
                    }
                }
            }
        }
    }
}