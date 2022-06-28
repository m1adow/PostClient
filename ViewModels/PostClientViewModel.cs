using PostClient.ViewModels.Infrastructure;
using PostClient.Models;
using PostClient.ViewModels.Helpers;
using System.Windows.Input;

namespace PostClient.ViewModels
{
    internal sealed class PostClientViewModel : ViewModelBase
    {
        public ICommand LoadedHandlerCommand { get; }

        public SendMessageViewModel SendMessageViewModel { get; }

        public LoadMessagesViewModel LoadMessagesViewModel { get; }

        public LoginViewModel LoginViewModel { get; }

        public ControlMessageViewModel ControlMessageViewModel { get; }

        private Account _account = new Account();

        public PostClientViewModel()
        {
            LoadedHandlerCommand = new RelayCommand(LoadedHandler);

            SendMessageViewModel = new SendMessageViewModel(GetAccount);
            LoadMessagesViewModel = new LoadMessagesViewModel(GetAccount);
            LoginViewModel = new LoginViewModel(ChangeAccountAfterLogining, LoadMessagesViewModel.LoadMessagesFromServerAction);
            ControlMessageViewModel = new ControlMessageViewModel(GetAccount, LoadMessagesViewModel.DeleteMessageFunc);
        }

        private Account GetAccount()
        {
            if (_account.Email == null && _account.Password == null && _account.PostServiceName == null)
                LoadAccount();

            return _account;
        }

        private async void LoadAccount()
        {
            try
            {
                _account = await JSONSaverAndReaderHelper.Read<Account>("AccountCredentials.json");
            }
            catch
            {
                MessageDialogShower.ShowMessageDialog("You have to login");
            }
        }

        private void ChangeAccountAfterLogining(Account account) => _account = account;

        private void LoadedHandler()
        {
            LoadAccount();
            LoadMessagesViewModel.LoadMessagesFromLocalStorageAction();
        }
    }
}
