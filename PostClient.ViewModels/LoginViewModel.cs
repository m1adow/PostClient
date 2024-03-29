﻿using PostClient.Models;
using PostClient.Models.Services;
using PostClient.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        private bool _isRememberMeChecked = true;

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

        private readonly Action<object> _loadMessagesAction;

        private readonly Func<Account, Task> _changeAccountFunc;

        private readonly Action<Account> _updateAccountControlsAction;

        private readonly Action<Visibility> _changeSearchBoxVisibilityAction;

        private Visibility? _loginButtonVisibility;

        private const string _fileName = "AccountsCredentials.json";

        public LoginViewModel(Func<Account, Task> changeAccount, Action<object> loadMessages, Action<Visibility> changeSearchBoxVisibilityAction, Visibility? loginButtonVisibility, Action<Account> updateAccountControls)
        {
            _changeAccountFunc = changeAccount;
            _loadMessagesAction = loadMessages;
            _updateAccountControlsAction = updateAccountControls;
            _changeSearchBoxVisibilityAction = changeSearchBoxVisibilityAction;
            _loginButtonVisibility = loginButtonVisibility;

            LoginCommand = new RelayCommand(LoginIntoAccount, IsLoginFieldsFilled);
            ShowLoginControlsCommand = new RelayCommand(ShowLoginControls);
            CancelLoginControlsCommand = new RelayCommand(HideLoginControls);
        }

        #region Login
        private async void LoginIntoAccount(object parameter)
        {
            try
            {
                Account account = new Account()
                {
                    Email = this.Email,
                    Password = this.Password,
                    PostServiceName = this.GetServiceName(),
                };

                HideLoginControls(parameter);
                _updateAccountControlsAction(account);
                await _changeAccountFunc(account);
                _loadMessagesAction(parameter);

                Account encryptedAccount = new Account
                {
                    Email = this.Email,
                    Password = EncryptionHelper.Encrypt(this.Password),
                    PostServiceName = this.GetServiceName()
                };

                if (IsRememberMeChecked)
                {
                    var accounts = await JSONSaverAndReaderHelper.Read<List<Account>>(_fileName);
                    accounts.Add(encryptedAccount);
                    await JSONSaverAndReaderHelper.Save(accounts, _fileName);
                }
                    
                ClearFields();
                (LoginCommand as RelayCommand)?.OnExecuteChanged(); //for disabling login button on second time
            }
            catch (MailKit.Security.AuthenticationException exception)
            {
                ContentDialogShower.ShowContentDialog("Error!", exception.Message);
            }
        }

        private string GetServiceName() => IsGmailRadioButtonChecked ? nameof(GmailService) : nameof(OutlookService);

        private void ClearFields()
        {
            Email = string.Empty;
            Password = string.Empty;
            IsOutlookRadioButtonChecked = false;
            IsGmailRadioButtonChecked = true;
            IsRememberMeChecked = true;
        }

        private bool IsLoginFieldsFilled(object parameter) => Email?.Length > 0 && Password?.Length > 0;
        #endregion       

        #region Showing login controls
        private void ShowLoginControls(object parameter)
        {
            ManagmentButtonsVisibility = Visibility.Collapsed;
            _loginButtonVisibility = Visibility.Collapsed;
            LoginControlsVisibility = Visibility.Visible;
            _changeSearchBoxVisibilityAction(Visibility.Collapsed);
        }
        #endregion

        #region Hiding login controls
        private void HideLoginControls(object parameter)
        {
            ManagmentButtonsVisibility = Visibility.Visible;
            LoginControlsVisibility = Visibility.Collapsed;
            _changeSearchBoxVisibilityAction(Visibility.Visible);
        }
        #endregion
    }
}
