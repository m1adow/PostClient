using PostClient.ViewModels.Infrastructure;
using PostClient.Models;
using PostClient.Models.Helpers;
using System.Windows.Input;
using PostClient.Models.Infrastructure;
using PostClient.Models.Services;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Linq;

namespace PostClient.ViewModels
{
#nullable enable

    public sealed class PostClientViewModel : ViewModelBase
    {       
        public ICommand LoadedHandlerCommand { get; }

        public SendMessageViewModel SendMessageViewModel { get; }

        public LoadMessagesViewModel LoadMessagesViewModel { get; }

        public LoginViewModel LoginViewModel { get; }

        public ControlMessageViewModel ControlMessageViewModel { get; }

        public AccountViewModel AccountViewModel { get; }

        private IPostService? _postService;

        private Account? _account;

        public PostClientViewModel()
        {
            LoadedHandlerCommand = new RelayCommand(LoadedHandler);

            LoadMessagesViewModel = new LoadMessagesViewModel(GetService);
            AccountViewModel = new AccountViewModel(SetAccount);
            LoginViewModel = new LoginViewModel(ChangeAccountAfterLogining, LoadMessagesViewModel.LoadMessagesFromServerAction, AccountViewModel.LoginButtonVisibility, AccountViewModel.AccountControlsVisibility, AccountViewModel.UpdateAccountControlsAction);
            SendMessageViewModel = new SendMessageViewModel(GetService, GetAccount, LoadMessagesViewModel.DeleteMessageFunc);
            ControlMessageViewModel = new ControlMessageViewModel(GetService, LoadMessagesViewModel.FlagMessageFunc, LoadMessagesViewModel.DeleteMessageFunc, SendMessageViewModel.ChangeSendMessageControlsVisibilityAndFillFieldsFunc, LoadMessagesViewModel.ArchiveMessageAction, LoadMessagesViewModel.UpdateMessagesAction);
        }

        private Account GetAccount() => _account;

        private void SetAccount(Account account) => _account = account;

        private IPostService GetService() => _postService;

        private void GenerateService()
        {
            if (_account != null && _account.Email != null && _account.Password != null)
            {
                _postService = _account.PostServiceName switch
                {
                    nameof(GmailService) => new GmailService(_account, MessageDialogShower.ShowMessageDialog),
                    nameof(OutlookService) => new OutlookService(_account, MessageDialogShower.ShowMessageDialog),
                    _ => throw new ArgumentNullException(_account.PostServiceName),
                };

                AccountViewModel.UpdateAccountControlsAction(_account);
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
                if (_account.Email == null || _account.Password == null)
                    throw new Exception();
                _account.Password = EncryptionHelper.Decrypt(_account.Password);
            }
            catch
            {
                MessageDialogShower.ShowMessageDialog("You have to login");
            }
        }
    }
}
