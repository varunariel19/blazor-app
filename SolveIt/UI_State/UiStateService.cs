
namespace SolveIt.UI_state
{
    public class UiStateService
    {
        public bool ShowAskForm { get; private set; }

        public event Action? OnChange;

        public void ToggleAskForm()
        {
            ShowAskForm = !ShowAskForm;
            Notify();
        }

        public void HideAskForm()
        {
            ShowAskForm = false;
            Notify();
        }


        private void Notify() => OnChange?.Invoke();
    }
}
