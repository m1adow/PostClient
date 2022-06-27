using PostClient.Models;
using PostClient.Models.Services;
using PostClient.ViewModels.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace PostClient.ViewModels
{
    internal sealed class LoginViewModel : ViewModelBase
    {
        private Visibility _managmentButtonsVisibility = Visibility.Visible;

        public Visibility ManagmentButtonsVisibility
        {
            get => _managmentButtonsVisibility;
            set => Set(ref _managmentButtonsVisibility, value);
        }

        private Visibility _loginControlsVisibility = Visibility.Collapsed;

        public Visibility LoginControlsVisibility
        {
            get => _loginControlsVisibility;
            set => Set(ref _loginControlsVisibility, value);
        }

        private bool _isRememberMeChecked = false;

        public bool IsRememberMeChecked
        {
            get => _isRememberMeChecked;
            set => Set(ref _isRememberMeChecked, value);
        }

        private string _email = string.Empty;

        public string Email
        {
            get => _email;
            set => Set(ref _email, value, new ICommand[] { LoginCommand });
        }

        private string _password = string.Empty;

        public string Password
        {
            get => _password;
            set => Set(ref _password, value, new ICommand[] { LoginCommand });
        }

        private bool _isGmailRadioButtonChecked = true;

        public bool IsGmailRadioButtonChecked
        {
            get => _isGmailRadioButtonChecked;
            set => Set(ref _isGmailRadioButtonChecked, value);
        }

        private bool _isOutlookRadioButtonChecked = false;

        public bool IsOutlookRadioButtonChecked
        {
            get => _isOutlookRadioButtonChecked;
            set => Set(ref _isOutlookRadioButtonChecked, value);
        }

        public ICommand LoginCommand { get; }

        public ICommand ShowLoginControlsCommand { get; }

        public ICommand CancelLoginControlsCommand { get; }

        private Account _account;

        private Action _loadMessages;

        private Action<Account> _changeAccount;

        public LoginViewModel(Action<Account> changeAccount, Action loadMessages)
        {
            _changeAccount = changeAccount;
            _loadMessages = loadMessages;

            LoginCommand = new RelayCommand(LoginIntoAccount, IsLoginFieldsFilled);
            ShowLoginControlsCommand = new RelayCommand(ShowLoginControls);
            CancelLoginControlsCommand = new RelayCommand(HideLoginControls);
        }

        #region Methods for login command
        private void LoginIntoAccount()
        {
            _account = new Account()
            {
                Email = this.Email,
                Password = this.Password,
                PostServiceName = GetServiceName()
            };

            if (IsRememberMeChecked)
                JSONSaverAndReaderHelper.Save(_account);

            ClearFields();
            HideLoginControls();
            _changeAccount(_account);
            _loadMessages();
        }

        private string GetServiceName()
        {
            if (IsGmailRadioButtonChecked)
                return nameof(GmailService);
            else
                return nameof(OutlookService);
        }

        private void ClearFields()
        {
            Email = string.Empty;
            Password = string.Empty;
        }

        private bool IsLoginFieldsFilled() => Email.Length > 0 && Password.Length > 0 && IsGmailRadioButtonChecked || IsOutlookRadioButtonChecked;
        #endregion       

        #region Method for showing login controls command
        private void ShowLoginControls()
        {
            ManagmentButtonsVisibility = Visibility.Collapsed;
            LoginControlsVisibility = Visibility.Visible;
        }
        #endregion

        #region Method for showing login controls command
        private void HideLoginControls()
        {
            ManagmentButtonsVisibility = Visibility.Visible;
            LoginControlsVisibility = Visibility.Collapsed;
        }
        #endregion
    }
}
