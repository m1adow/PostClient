using PostClient.Models;
using PostClient.Models.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace PostClient.ViewModels
{
#nullable enable

    public class AccountViewModel : ViewModelBase
    {
        private Visibility? _accountControlsVisibility = Visibility.Collapsed;

        public Visibility? AccountControlsVisibility
        {
            get => _accountControlsVisibility;
            set => Set(ref _accountControlsVisibility, value);
        }

        private Visibility? _loginButtonVisibility = Visibility.Visible;

        public Visibility? LoginButtonVisibility
        {
            get => _loginButtonVisibility;
            set => Set(ref _loginButtonVisibility, value);
        }

        private string? _email = string.Empty;

        public string? Email
        {
            get => _email;
            set => Set(ref _email, value);
        }

        private string? _service = string.Empty;

        public string? Service
        {
            get => _service;
            set => Set(ref _service, value);
        }

        public ICommand LogoutCommand { get; }

        public ICommand ShowAccountInfoCommand { get; }

        public Action<Account> UpdateAccountControlsAction { get; }

        private readonly Action<Account> _setAccount;

        private readonly Action _clearMessages;

        public AccountViewModel(Action<Account> setAccount, Action clearMessages)
        {
            _setAccount = setAccount;
            _clearMessages = clearMessages;
            UpdateAccountControlsAction = UpdateAccountControls;

            LogoutCommand = new RelayCommand(Logout);
            ShowAccountInfoCommand = new RelayCommand(ShowAccountInfo);
        }

        private async void Logout(object parameter)
        {
            AccountControlsVisibility = Visibility.Collapsed;
            LoginButtonVisibility = Visibility.Visible;

            Account account = new Account();
            _setAccount(account);
            await JSONSaverAndReaderHelper.Save(account, "AccountCredentials.json");
            await ClearAllMessages();
        }

        private async Task ClearAllMessages()
        {
            var folders = new string[] { "AllMessages.json", "ArchiveMessages.json", "DraftMessages.json", "FlaggedMessages.json", "SentMessages.json" };

            foreach (string folder in folders)
                await JSONSaverAndReaderHelper.Save(new List<MailMessage>(), folder);

            _clearMessages();
        }

        private void ShowAccountInfo(object parameter) => ContentDialogShower.ShowMessageDialog("Account", $"Email: {Email}\nService: {Service}");

        private void UpdateAccountControls(Account account)
        {
            AccountControlsVisibility = Visibility.Visible;
            LoginButtonVisibility = Visibility.Collapsed;
            Email = new string(account.Email.TakeWhile(a => a != '@').ToArray());
            Service = account.PostServiceName.Replace("Service", "");
        }
    }
}
