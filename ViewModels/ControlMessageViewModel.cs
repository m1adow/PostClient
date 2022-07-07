using PostClient.Models;
using PostClient.Models.Services;
using PostClient.ViewModels.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Threading.Tasks;
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

                if (value.Body.Length > 0 && !value.IsDraft)
                    MessageViewConrtolVisibility = Visibility.Visible;
                else if (value.IsDraft)
                    _changeSendMessageControlsVisibilityAndMessage(Visibility.Visible, value);
            }
        }

        private Visibility _messageViewConrtolVisibility = Visibility.Collapsed;

        public Visibility MessageViewConrtolVisibility
        {
            get => _messageViewConrtolVisibility;
            set => Set(ref _messageViewConrtolVisibility, value);
        }

        public ICommand FlagMessageCommand { get; }

        public ICommand DeleteMessageCommand { get; }

        public ICommand CloseMessageCommand { get; }

        public ICommand HideMessageViewCommand { get; }

        private readonly Func<Account> _getAccount;
        private readonly Func<MailMessage, Task<bool>> _updateFlaggedList;
        private readonly Func<MailMessage, bool> _deleteMessageFromList;
        private readonly Func<Visibility, MailMessage, bool> _changeSendMessageControlsVisibilityAndMessage;

        public ControlMessageViewModel(Func<Account> getAccount, Func<MailMessage, Task<bool>> updateFlaggedList, Func<MailMessage, bool> deleteMessageFromList, Func<Visibility, MailMessage, bool> changeSendMessageControlsVisibilityAndMessage)
        {
            _getAccount = getAccount;
            _updateFlaggedList = updateFlaggedList;
            _deleteMessageFromList = deleteMessageFromList;
            _changeSendMessageControlsVisibilityAndMessage = changeSendMessageControlsVisibilityAndMessage;

            FlagMessageCommand = new RelayCommand(FlagMessage);
            DeleteMessageCommand = new RelayCommand(DeleteMessage);
            CloseMessageCommand = new RelayCommand(CloseMessage);
            HideMessageViewCommand = new RelayCommand(HideMessageView);
        }

        #region Methods for flag message
        private void FlagMessage(object parameter)
        {
            Account account = _getAccount();

            switch (account.PostServiceName)
            {
                case nameof(GmailService):
                    new GmailService(account).FlagMessage(SelectedMailMessage, MessageDialogShower.ShowMessageDialog);
                    break;
                case nameof(OutlookService):
                    new OutlookService(account).FlagMessage(SelectedMailMessage, MessageDialogShower.ShowMessageDialog);
                    break;
            }

            _updateFlaggedList(SelectedMailMessage);
        }
        #endregion

        #region Methods for deleting message
        private void DeleteMessage(object parameter)
        {
            Account account = _getAccount();

            switch (account.PostServiceName)
            {
                case nameof(GmailService):
                    new GmailService(account).DeleteMessage(SelectedMailMessage, MessageDialogShower.ShowMessageDialog);
                    break;
                case nameof(OutlookService):
                    new OutlookService(account).DeleteMessage(SelectedMailMessage, MessageDialogShower.ShowMessageDialog);
                    break;
            }

            _deleteMessageFromList(SelectedMailMessage);
            CloseMessage(parameter);
        }
        #endregion

        #region Method for closing message
        private void CloseMessage(object parameter)
        {
            _selectedMailMessage = new MailMessage();

            MessageViewConrtolVisibility = Visibility.Collapsed;
            _changeSendMessageControlsVisibilityAndMessage(Visibility.Collapsed, _selectedMailMessage);
        }
        #endregion

        #region Method for hiding message view
        private void HideMessageView(object parameter) => MessageViewConrtolVisibility = Visibility.Collapsed;
        #endregion
    }
}
