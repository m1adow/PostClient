using PostClient.ViewModels.Infrastructure;
using PostClient.Models;
using PostClient.Helpers;
using System.Windows.Input;
using PostClient.Models.Infrastructure;
using PostClient.Models.Services;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace PostClient.ViewModels
{
#nullable enable

    public sealed class PostClientViewModel : ViewModelBase
    {
        private ObservableCollection<Account>? _accounts = new ObservableCollection<Account>();

        public ObservableCollection<Account>? Accounts
        {
            get => _accounts;
            set => Set(ref _accounts, value);
        }

        private Account? _selectedAccount = new Account();

        public Account? SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                Set(ref _selectedAccount, value);
                LoadMessagesViewModel.LoadMessagesFromLocalStorageCommand.Execute("AllMessages");
            }
        }

        public ICommand LoadedHandlerCommand { get; }

        public SendMessageViewModel SendMessageViewModel { get; }

        public LoadMessagesViewModel LoadMessagesViewModel { get; }

        public LoginViewModel LoginViewModel { get; }

        public ControlMessageViewModel ControlMessageViewModel { get; }

        public AccountViewModel AccountViewModel { get; }

        public KeyDownImplementationViewModel KeyDownImplementationViewModel { get; }

        public AnimationsImplementationViewModel AnimationsImplementationViewModel { get; } 

        private IPostService? _postService;

        public PostClientViewModel()
        {
            LoadedHandlerCommand = new RelayCommand(LoadedHandler);

            LoadMessagesViewModel = new LoadMessagesViewModel(GetService, GetAccount);
            AccountViewModel = new AccountViewModel(SetAccount, LoadMessagesViewModel.ClearMessagesAction, ClearService);
            SendMessageViewModel = new SendMessageViewModel(GetService, GetAccount, LoadMessagesViewModel.DeleteMessageFunc);
            ControlMessageViewModel = new ControlMessageViewModel(GetService, LoadMessagesViewModel.FlagMessageFunc, LoadMessagesViewModel.DeleteMessageFunc, SendMessageViewModel.ChangeSendMessageControlsVisibilityAndFillFieldsFunc, LoadMessagesViewModel.ArchiveMessageAction, LoadMessagesViewModel.UpdateMessagesAction);
            LoginViewModel = new LoginViewModel(ChangeAccountAfterLogining, LoadMessagesViewModel.LoadMessagesFromServerAction, ControlMessageViewModel.ChangeSearchBoxControlVisibilityAction, AccountViewModel.LoginButtonVisibility, AccountViewModel.UpdateAccountControlsAction);
            KeyDownImplementationViewModel = new KeyDownImplementationViewModel(ControlMessageViewModel.DeleteMessageCommand.Execute, ControlMessageViewModel.FlagMessageCommand.Execute, ControlMessageViewModel.UnseenMessageCommand.Execute, ControlMessageViewModel.ArchiveMessageCommand.Execute, LoadMessagesViewModel.LoadMessagesFromServerCommand.Execute);
            AnimationsImplementationViewModel = new AnimationsImplementationViewModel();
        }

        private Account GetAccount() => SelectedAccount;

        private void SetAccount(Account account) => _accounts.Add(account);

        private IPostService GetService() => _postService;

        private async Task GenerateService(Account account)
        {
            if (account != null && account.Email != string.Empty && account.Password != string.Empty)
            {
                _postService = account.PostServiceName switch
                {
                    nameof(GmailService) => await GmailService.CreateAsync(account, ContentDialogShower.ShowContentDialog),
                    nameof(OutlookService) => await OutlookService.CreateAsync(account, ContentDialogShower.ShowContentDialog),
                    _ => throw new ArgumentNullException(account.PostServiceName),
                };

                AccountViewModel.UpdateAccountControlsAction(account);
                SendMessageViewModel.MessageSender = account.Email;
                (LoadMessagesViewModel.LoadMessagesFromServerCommand as RelayCommand)?.OnExecuteChanged();
                (SendMessageViewModel.ShowSendingControlsCommand as RelayCommand)?.OnExecuteChanged();
                if (!Accounts.Contains(account))
                    Accounts?.Add(account);
            }
        }

        private async Task ClearService()
        {
            await _postService.CloseClients();
            _postService = null;
            (LoadMessagesViewModel.LoadMessagesFromServerCommand as RelayCommand)?.OnExecuteChanged();
            (SendMessageViewModel.ShowSendingControlsCommand as RelayCommand)?.OnExecuteChanged();
        }

        private async Task ChangeAccountAfterLogining(Account account)
        {
            await GenerateService(account);
            SelectedAccount = account;
        }

        private async void LoadedHandler(object parameter)
        {
            await LoadAccount();
            await GenerateService(SelectedAccount);
            LoadMessagesViewModel.LoadMessagesFromLocalStorageAction(parameter);
        }

        private async Task LoadAccount()
        {
            try
            {
                var accounts = await JSONSaverAndReaderHelper.Read<List<Account>>("AccountsCredentials.json");
                if (accounts.Count != 0)
                {
                    Accounts = new ObservableCollection<Account>(accounts);
                    SelectedAccount = accounts.FirstOrDefault(a => a.Email != null && a.Password != null);
                    if (SelectedAccount.Email == null || SelectedAccount.Password == null)
                        throw new Exception();
                    SelectedAccount.Password = EncryptionHelper.Decrypt(SelectedAccount.Password);
                }
                else
                    throw new Exception();
            }
            catch
            {
                ContentDialogShower.ShowContentDialog("Warning!", "You have to login");
            }
        }
    }
}
