using PostClient.ViewModels.Infrastructure;
using PostClient.Models;
using PostClient.ViewModels.Helpers;
using System.Windows.Input;
using PostClient.Models.Infrastructure;
using PostClient.Models.Services;
using System;
using System.Threading.Tasks;

namespace PostClient.ViewModels
{
    #nullable enable

    internal sealed class PostClientViewModel : ViewModelBase
    {
        public ICommand LoadedHandlerCommand { get; }

        public SendMessageViewModel SendMessageViewModel { get; }

        public LoadMessagesViewModel LoadMessagesViewModel { get; }

        public LoginViewModel LoginViewModel { get; }

        public ControlMessageViewModel ControlMessageViewModel { get; }

        private IPostService? _service;

        private Account? _account;

        public PostClientViewModel()
        {
            LoadedHandlerCommand = new RelayCommand(LoadedHandler);

            LoadMessagesViewModel = new LoadMessagesViewModel(GetService);
            LoginViewModel = new LoginViewModel(ChangeAccountAfterLogining, LoadMessagesViewModel.LoadMessagesFromServerAction);
            SendMessageViewModel = new SendMessageViewModel(GetService, GetAccount, LoadMessagesViewModel.DeleteMessageFunc);
            ControlMessageViewModel = new ControlMessageViewModel(GetService, LoadMessagesViewModel.FlagMessageFunc, LoadMessagesViewModel.DeleteMessageFunc, SendMessageViewModel.ChangeSendMessageControlsVisibilityAndFillFieldsFunc);
        }

        private Account GetAccount() => _account;

        private IPostService GetService() => _service;

        private void GenerateService()
        {
            if (_account != null)
            {
                _service = _account.PostServiceName switch
                {
                    nameof(GmailService) => new GmailService(_account),
                    nameof(OutlookService) => new OutlookService(_account),
                    _ => throw new ArgumentNullException(_account.PostServiceName),
                };
            }
        }

        private void ChangeAccountAfterLogining(Account account)
        {
            _account = account;
            GenerateService();
        }

        private async void LoadedHandler(object parameter)
        {
            await LoadAccount();
            GenerateService();
            LoadMessagesViewModel.LoadMessagesFromLocalStorageAction(parameter);
        }

        private async Task LoadAccount()
        {
            try
            {
                _account = await JSONSaverAndReaderHelper.Read<Account>("AccountCredentials.json");
                _account.Password = EncryptionHelper.Decrypt(_account.Password);
            }
            catch
            {
                MessageDialogShower.ShowMessageDialog("You have to login");
            }
        }
    }
}
