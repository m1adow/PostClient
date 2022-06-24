using PostClient.Models;
using PostClient.Models.Services;
using PostClient.ViewModels.Helpers;
using PostClient.ViewModels.Infrastructure;
using System.Windows.Input;

namespace PostClient.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _email = string.Empty;

        public string Email
        {
            get => _email;
            set => Set(ref _email, value, new ICommand[] { LoginCommand });
        }

        private string _password = string.Empty;

        public string Password
        {
            get => _password;
            set => Set(ref _password, value, new ICommand[] { LoginCommand });
        }

        private bool _isGmailRadioButtonChecked = true;

        public bool IsGmailRadioButtonChecked
        {
            get => _isGmailRadioButtonChecked;
            set => Set(ref _isGmailRadioButtonChecked, value);
        }

        private bool _isOutlookRadioButtonChecked = false;

        public bool IsOutlookRadioButtonChecked
        {
            get => _isOutlookRadioButtonChecked;
            set => Set(ref _isOutlookRadioButtonChecked, value);
        }

        public ICommand LoginCommand { get; private set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(LoginIntoAccount, IsFieldsFilled);
        }

        #region Methods for login command
        private void LoginIntoAccount()
        {
            Account account = new Account()
            {
                Email = this.Email,
                Password = this.Password,
                PostServiceName = GetServiceName()
            };

            JSONSaverAndReaderHelper.Save(account);
            ClearFields();
        }

        private string GetServiceName()
        {
            if (IsGmailRadioButtonChecked)
                return nameof(GmailService);
            else
                return nameof(OutlookService);
        }

        private void ClearFields()
        {
            Email = string.Empty;
            Password = string.Empty;
        }

        private bool IsFieldsFilled() => Email.Length > 0 && Password.Length > 0 && IsGmailRadioButtonChecked || IsOutlookRadioButtonChecked;
        #endregion
    }
}
