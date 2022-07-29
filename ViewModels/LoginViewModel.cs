using PostClient.Models;
using PostClient.Models.Services;
using PostClient.Models.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace PostClient.ViewModels
{
#nullable enable

    public sealed class LoginViewModel : ViewModelBase
    {
        private Visibility? _managmentButtonsVisibility = Visibility.Visible;

        public Visibility? ManagmentButtonsVisibility
        {
            get => _managmentButtonsVisibility;
            set => Set(ref _managmentButtonsVisibility, value);
        }

        private Visibility? _loginControlsVisibility = Visibility.Collapsed;

        public Visibility? LoginControlsVisibility
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

        private string? _email = string.Empty;

        public string? Email
        {
            get => _email;
            set => Set(ref _email, value, new ICommand[] { LoginCommand });
        }

        private string? _password = string.Empty;

        public string? Password
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

        private readonly Action<object> _loadMessages;

        private readonly Action<Account> _changeAccount;

        private readonly Action<Account> _updateAccountControls;

        private Visibility? _loginButtonVisibility;

        private Visibility? _accountControlsVisibility;

        public LoginViewModel(Action<Account> changeAccount, Action<object> loadMessages, Visibility? loginButtonVisibility, Visibility? accountControlsVisibility, Action<Account> updateAccountControls)
        {
            _changeAccount = changeAccount;
            _loadMessages = loadMessages;
            _updateAccountControls = updateAccountControls;

            LoginCommand = new RelayCommand(LoginIntoAccount, IsLoginFieldsFilled);
            ShowLoginControlsCommand = new RelayCommand(ShowLoginControls);
            CancelLoginControlsCommand = new RelayCommand(HideLoginControls);
            _loginButtonVisibility = loginButtonVisibility;
        }

        #region Login
        private void LoginIntoAccount(object parameter)
        {
            try
            {
                Account account = new Account()
                {
                    Email = this.Email,
                    Password = this.Password,
                    PostServiceName = this.GetServiceName()
                };

                _changeAccount(account);
                _loadMessages(parameter);
                _updateAccountControls(account);

                Account encryptedAccount = new Account
                {
                    Email = this.Email,
                    Password = EncryptionHelper.Encrypt(this.Password),
                    PostServiceName = this.GetServiceName()
                };

                if (IsRememberMeChecked)
                    JSONSaverAndReaderHelper.Save(encryptedAccount, "AccountCredentials.json");

                ClearFields();
                HideLoginControls(parameter);
                (LoginCommand as RelayCommand)?.OnExecuteChanged(); //for disabling login button on second time
            }
            catch (MailKit.Security.AuthenticationException exception)
            {
                MessageDialogShower.ShowMessageDialog(exception.Message);
            }
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

        private bool IsLoginFieldsFilled(object parameter) => Email?.Length > 0 && Password?.Length > 0 && (IsGmailRadioButtonChecked || IsOutlookRadioButtonChecked);
        #endregion       

        #region Showing login controls
        private void ShowLoginControls(object parameter)
        {
            ManagmentButtonsVisibility = Visibility.Collapsed;
            _loginButtonVisibility = Visibility.Collapsed;
            LoginControlsVisibility = Visibility.Visible;
        }
        #endregion

        #region Hiding login controls
        private void HideLoginControls(object parameter)
        {
            ManagmentButtonsVisibility = Visibility.Visible;
            LoginControlsVisibility = Visibility.Collapsed;
        }
        #endregion
    }
}
