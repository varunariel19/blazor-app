using SolveIt.Models;
using SolveIt.Services;


namespace SolveIt.UI_state
{
    public class UiStateService
    {
        #region Variables 
        private List<Question> _questions = [];
        public List<Question> Questions => _questions;

        private List<ConversationService.InboxMessage> _inboxMessages = [];
        public List<ConversationService.InboxMessage> InboxMessages => _inboxMessages;

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

        public async Task HandleUserLogin(User user)
        {
            IsLoggedIn = true;
            _user = user; 
            await NotifyUserLoaded(); 
        }


        public void ArrangeOrInsertInboxItem(ConversationService.InboxMessage inboxMessage)
        {
            var idx = _inboxMessages.FindIndex(x => x.InboxId == inboxMessage.InboxId);

            if(idx != -1)
            {
                MoveToTop(idx);
            }
            else
            {
                _inboxMessages.Insert(0 , inboxMessage);
            }

            Notify();
        }

        public void SetInbox(List<ConversationService.InboxMessage> inboxMessages)
        {
            _inboxMessages = inboxMessages;
            Notify();
        }


        public void ArrangeInboxItemsOrder()
        {
            _inboxMessages = [.. _inboxMessages.OrderByDescending(x => x.ReceivedAt)];
            Notify();
        }

        public void MoveToTop(int idx)
        {
            if (idx <= 0) return;

            var item = _inboxMessages[idx];
            _inboxMessages.RemoveAt(idx);
            _inboxMessages.Insert(0, item);

        }


        #endregion








        private void Notify() => OnChange?.Invoke();

        private async Task NotifyUserLoaded()
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