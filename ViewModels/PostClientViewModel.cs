using PostClient.ViewModels.Infrastructure;
using PostClient.Models;
using PostClient.ViewModels.Helpers;
using System.Windows.Input;

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

        private Account? _account;

        public PostClientViewModel()
        {
            LoadedHandlerCommand = new RelayCommand(LoadedHandler);

            LoadMessagesViewModel = new LoadMessagesViewModel(GetAccount);
            LoginViewModel = new LoginViewModel(ChangeAccountAfterLogining, LoadMessagesViewModel.LoadMessagesFromServerAction);
            SendMessageViewModel = new SendMessageViewModel(GetAccount, LoadMessagesViewModel.DeleteMessageFunc);
            ControlMessageViewModel = new ControlMessageViewModel(GetAccount, LoadMessagesViewModel.FlagMessageFunc, LoadMessagesViewModel.DeleteMessageFunc, SendMessageViewModel.ChangeSendMessageControlsVisibilityAndFillFieldsFunc);
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
            }
            catch
            {
                MessageDialogShower.ShowMessageDialog("You have to login");
            }
        }

        private void ChangeAccountAfterLogining(Account account) => _account = account;

        private void LoadedHandler(object parameter) => LoadMessagesViewModel.LoadMessagesFromLocalStorageAction(parameter);
    }
}
