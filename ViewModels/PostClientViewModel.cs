using PostClient.ViewModels.Infrastructure;
using PostClient.Models;
using PostClient.ViewModels.Helpers;
using System.Windows.Input;
using PostClient.Models.Infrastructure;
using PostClient.Models.Services;
using System;

namespace PostClient.ViewModels
{
#nullable enable

    internal sealed class PostClientViewModel : ViewModelBase
    {
        public ICommand LoadedHandlerCommand { get; }

        public ICommand ClosedHandlerCommand { get; }

        public SendMessageViewModel SendMessageViewModel { get; }

        public LoadMessagesViewModel LoadMessagesViewModel { get; }

        public LoginViewModel LoginViewModel { get; }

        public ControlMessageViewModel ControlMessageViewModel { get; }

        public IPostService? Service { get; private set; }

        private Account? _account;

        public PostClientViewModel()
        {
            LoadedHandlerCommand = new RelayCommand(LoadedHandler);
            ClosedHandlerCommand = new RelayCommand(ClosedHandler);

            LoadMessagesViewModel = new LoadMessagesViewModel(GetService, GetAccount);
            LoginViewModel = new LoginViewModel(ChangeAccountAfterLogining, LoadMessagesViewModel.LoadMessagesFromServerAction);
            SendMessageViewModel = new SendMessageViewModel(GetService, GetAccount, LoadMessagesViewModel.DeleteMessageFunc);
            ControlMessageViewModel = new ControlMessageViewModel(GetService, GetAccount, LoadMessagesViewModel.FlagMessageFunc, LoadMessagesViewModel.DeleteMessageFunc, SendMessageViewModel.ChangeSendMessageControlsVisibilityAndFillFieldsFunc);
        }

        private Account GetAccount()
        {
            if (_account == null)
                LoadAccount();

            return _account;
        }

        private async void LoadAccount()
        {
            try
            {
                _account = await JSONSaverAndReaderHelper.Read<Account>("AccountCredentials.json");
                _account.Password = EncryptionHelper.Decrypt(_account.Password);
                Service = GetService(_account);
            }
            catch
            {
                MessageDialogShower.ShowMessageDialog("You have to login");
            }
        }

        private IPostService GetService(Account account)
        {
            if (Service == null)
                Service = GenerateService(account);

            return Service;
        }

        private IPostService GenerateService(Account account)
        {
            return account.PostServiceName switch
            {
                nameof(GmailService) => new GmailService(account),
                nameof(OutlookService) => new OutlookService(account),
                _ => throw new ArgumentNullException(account.PostServiceName)
            };
        }

        private void ChangeAccountAfterLogining(Account account)
        {
            _account = account;
            Service = GenerateService(account);
        }

        private void LoadedHandler(object parameter)
        {
            LoadMessagesViewModel.LoadMessagesFromLocalStorageAction(parameter);
            LoadAccount();
        }

        private void ClosedHandler(object parameter) => Service.CloseClients();
    }
}
