
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using System.Security.Claims;
using SolveIt.Models;
using Microsoft.AspNetCore.Authentication;

namespace SolveIt.UI_state
{
    public class UiStateService
    {

        #region Variables 
        public List<Question> Questions => _questions; // readonly from outside 
        private List<Question> _questions = [];  // can't  access from outside 
        public bool IsLoggedIn = false;

        private User? _user = null;
        public User? UserData => _user;

        public event Action? OnChange;

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
        public void ToggleAskForm()
        {
            ShowAskForm = !ShowAskForm;
        }

        public void HideAskForm()
        {
            ShowAskForm = false;
        }
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




        /// <summary>
        ///  set this when the user logs in and access from here using @inject
        /// </summary>
        public void PresistUserState(User user)
        {
            _user = user;
            Notify();
        }

        /// <summary>
        /// Remove when user log out !! 
        /// </summary>
        public void DestroyUserState()
        {
            _user = null;
            Notify();
        }

        public void HandleUserLogin(User user)
        {
            IsLoggedIn = true;
            PresistUserState(user);
        }

      

        #endregion


        private void Notify() => OnChange?.Invoke();


    }


}