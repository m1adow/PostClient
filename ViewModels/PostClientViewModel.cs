using PostClient.ViewModels.Infrastructure;
using PostClient.Models;
using PostClient.Models.Helpers;
using System.Windows.Input;
using PostClient.Models.Infrastructure;
using PostClient.Models.Services;
using System;
using System.Threading.Tasks;

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

        public KeyDownImplementationViewModel KeyDownImplementationViewModel { get; }

        public AnimationsImplementationViewModel AnimationsImplementationViewModel { get; } 

        private IPostService? _postService;

        private Account? _account;

        public PostClientViewModel()
        {
            LoadedHandlerCommand = new RelayCommand(LoadedHandler);

            LoadMessagesViewModel = new LoadMessagesViewModel(GetService);
            AccountViewModel = new AccountViewModel(SetAccount, LoadMessagesViewModel.ClearMessagesAction);
            LoginViewModel = new LoginViewModel(ChangeAccountAfterLogining, LoadMessagesViewModel.LoadMessagesFromServerAction, AccountViewModel.LoginButtonVisibility, AccountViewModel.AccountControlsVisibility, AccountViewModel.UpdateAccountControlsAction);
            SendMessageViewModel = new SendMessageViewModel(GetService, GetAccount, LoadMessagesViewModel.DeleteMessageFunc);
            ControlMessageViewModel = new ControlMessageViewModel(GetService, LoadMessagesViewModel.FlagMessageFunc, LoadMessagesViewModel.DeleteMessageFunc, SendMessageViewModel.ChangeSendMessageControlsVisibilityAndFillFieldsFunc, LoadMessagesViewModel.ArchiveMessageAction, LoadMessagesViewModel.UpdateMessagesAction);
            KeyDownImplementationViewModel = new KeyDownImplementationViewModel(ControlMessageViewModel.DeleteMessageCommand.Execute, ControlMessageViewModel.FlagMessageCommand.Execute, ControlMessageViewModel.UnseenMessageCommand.Execute, ControlMessageViewModel.ArchiveMessageCommand.Execute, LoadMessagesViewModel.LoadMessagesFromServerCommand.Execute);
            AnimationsImplementationViewModel = new AnimationsImplementationViewModel();
        }

        private Account GetAccount() => _account;

        private void SetAccount(Account account) => _account = account;

        private IPostService GetService() => _postService;

        private async Task GenerateService(Account account)
        {
            if (account != null && account.Email != string.Empty && account.Password != string.Empty)
            {
                _postService = account.PostServiceName switch
                {
                    nameof(GmailService) => await GmailService.CreateAsync(account, ContentDialogShower.ShowMessageDialog),
                    nameof(OutlookService) => await OutlookService.CreateAsync(account, ContentDialogShower.ShowMessageDialog),
                    _ => throw new ArgumentNullException(account.PostServiceName),
                };

                AccountViewModel.UpdateAccountControlsAction(account);
                SendMessageViewModel.MessageSender = account.Email;
            }
        }

        private async void ChangeAccountAfterLogining(Account account)
        {
            await GenerateService(account);
            _account = account;
        }

        private async void LoadedHandler(object parameter)
        {
            await LoadAccount();
            await GenerateService(_account);
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
                ContentDialogShower.ShowMessageDialog("Warning!", "You have to login");
            }
        }
    }
}
