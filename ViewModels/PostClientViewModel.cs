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

        public ICommand LoadedHandlerCommand { get; }

        public ICommand LogoutCommand { get; }

        public SendMessageViewModel SendMessageViewModel { get; }

        public LoadMessagesViewModel LoadMessagesViewModel { get; }

        public LoginViewModel LoginViewModel { get; }

        public ControlMessageViewModel ControlMessageViewModel { get; }

        private IPostService? _postService;

        private Account? _account;

        public PostClientViewModel()
        {
            LoadedHandlerCommand = new RelayCommand(LoadedHandler);
            LogoutCommand = new RelayCommand(Logout);

            LoadMessagesViewModel = new LoadMessagesViewModel(GetService);
            LoginViewModel = new LoginViewModel(ChangeAccountAfterLogining, LoadMessagesViewModel.LoadMessagesFromServerAction, LoginButtonVisibility);
            SendMessageViewModel = new SendMessageViewModel(GetService, GetAccount, LoadMessagesViewModel.DeleteMessageFunc);
            ControlMessageViewModel = new ControlMessageViewModel(GetService, LoadMessagesViewModel.FlagMessageFunc, LoadMessagesViewModel.DeleteMessageFunc, SendMessageViewModel.ChangeSendMessageControlsVisibilityAndFillFieldsFunc);
        }

        private Account GetAccount() => _account;

        private IPostService GetService() => _postService;

        private void GenerateService()
        {
            if (_account != null && _account.Email != null && _account.Password != null)
            {
                _postService = _account.PostServiceName switch
                {
                    nameof(GmailService) => new GmailService(_account),
                    nameof(OutlookService) => new OutlookService(_account),
                    _ => throw new ArgumentNullException(_account.PostServiceName),
                };

                AccountControlsVisibility = Visibility.Visible;
                LoginButtonVisibility = Visibility.Collapsed;
                _account.Email.TakeWhile(a => a != '@').ToList().ForEach(x => Email += x);
                Service = _account.PostServiceName.Replace("Service", "");
            }
        }

        private void ChangeAccountAfterLogining(Account account)
        {
            _account = account;
            AccountControlsVisibility = Visibility.Visible;
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

        private void Logout(object parameter)
        {
            AccountControlsVisibility = Visibility.Collapsed;
            LoginButtonVisibility = Visibility.Visible;

            _account = new Account();
            JSONSaverAndReaderHelper.Save(_account, "AccountCredentials.json");
        }
    }
}
