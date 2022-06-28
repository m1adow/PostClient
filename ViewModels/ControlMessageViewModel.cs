using PostClient.Models;
using PostClient.Models.Services;
using PostClient.ViewModels.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace PostClient.ViewModels
{
    internal sealed class ControlMessageViewModel : ViewModelBase
    {
        private MailMessage _selectedMailMessage = new MailMessage();

        public MailMessage SelectedMailMessage
        {
            get => _selectedMailMessage;
            set
            {
                if (value == null)
                    value = new MailMessage();

                Set(ref _selectedMailMessage, value);

                if (_selectedMailMessage.Body.Length > 0)
                    MessageBodyControlsVisibility = Visibility.Visible;
            }
        }

        private Visibility _messageBodyControlsVisibility = Visibility.Collapsed;

        public Visibility MessageBodyControlsVisibility
        {
            get => _messageBodyControlsVisibility;
            set => Set(ref _messageBodyControlsVisibility, value);
        }

        public ICommand FlagMessageCommand { get; }

        public ICommand DeleteMessageCommand { get; }

        public ICommand CloseMessageCommand { get; }

        private Func<Account> _getAccount;
        private Func<MailMessage, bool> _addMessageToFlaggedList;
        private Func<MailMessage, bool> _deleteMessageFromList;

        public ControlMessageViewModel(Func<Account> getAccount, Func<MailMessage, bool> addMessageToFlaggedList, Func<MailMessage, bool> deleteMessageFromList)
        {
            _getAccount = getAccount;
            _addMessageToFlaggedList = addMessageToFlaggedList;
            _deleteMessageFromList = deleteMessageFromList;

            FlagMessageCommand = new RelayCommand(FlagMessage);
            DeleteMessageCommand = new RelayCommand(DeleteMessage);
            CloseMessageCommand = new RelayCommand(CloseMessage);
        }

        #region Methods for flag message
        private void FlagMessage()
        {
            Account account = _getAccount();

            switch (account.PostServiceName)
            {
                case nameof(GmailService):
                    new GmailService().FlagMessage(account, SelectedMailMessage, MessageDialogShower.ShowMessageDialog);
                    break;
                case nameof(OutlookService):
                    new OutlookService().FlagMessage(account, SelectedMailMessage, MessageDialogShower.ShowMessageDialog);
                    break;
            }

            _addMessageToFlaggedList(SelectedMailMessage);
        }
        #endregion

        #region Methods for deleting message
        private void DeleteMessage()
        {
            Account account = _getAccount();

            switch (account.PostServiceName)
            {
                case nameof(GmailService):
                    new GmailService().DeleteMessage(account, SelectedMailMessage, MessageDialogShower.ShowMessageDialog);
                    break;
                case nameof(OutlookService):
                    new OutlookService().DeleteMessage(account, SelectedMailMessage, MessageDialogShower.ShowMessageDialog);
                    break;
            }

            _deleteMessageFromList(SelectedMailMessage);
            CloseMessage();
        }
        #endregion

        #region Method for closing message
        private void CloseMessage()
        {
            _selectedMailMessage = new MailMessage();

            MessageBodyControlsVisibility = Visibility.Collapsed;
        }
        #endregion
    }
}
