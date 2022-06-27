using PostClient.ViewModels.Infrastructure;
using PostClient.Models;
using PostClient.ViewModels.Helpers;

namespace PostClient.ViewModels
{
    internal sealed class PostClientViewModel : ViewModelBase
    {
        private Account _account = new Account();

        public SendMessageViewModel SendMessageViewModel { get; } 

        public LoadMessagesViewModel LoadMessagesViewModel { get; }

        public LoginViewModel LoginViewModel { get; } 

        public PostClientViewModel()
        {
            SendMessageViewModel = new SendMessageViewModel(GetAccount);
            LoadMessagesViewModel = new LoadMessagesViewModel(GetAccount);
            LoginViewModel = new LoginViewModel(ChangeAccountAfterLogining, LoadMessagesViewModel.LoadMessagesAction);  
        }

        private Account GetAccount()
        {
            if (_account.Email == null && _account.Password == null && _account.PostServiceName == null)
                LoadAccount();

            return _account;
        }

        private async void LoadAccount() => _account = await JSONSaverAndReaderHelper.Read();

        private void ChangeAccountAfterLogining(Account account) => _account = account;        
    }
}
