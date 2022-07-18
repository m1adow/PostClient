using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using PostClient.Models;
using PostClient.Models.Infrastructure;
using PostClient.Models.Services;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace PostClient.ViewModels
{
    #nullable enable

    internal sealed class ControlMessageViewModel : ViewModelBase
    {       
        private MailMessage? _selectedMailMessage = new MailMessage();

        public MailMessage? SelectedMailMessage
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

        private Visibility? _messageViewConrtolVisibility = Visibility.Collapsed;

        public Visibility? MessageViewConrtolVisibility
        {
            get => _messageViewConrtolVisibility;
            set => Set(ref _messageViewConrtolVisibility, value);
        }

        public ICommand FlagMessageCommand { get; }

        public ICommand DeleteMessageCommand { get; }

        public ICommand CloseMessageCommand { get; }

        public ICommand HideMessageViewCommand { get; }


        private readonly Func<MailMessage, Task<bool>> _updateFlaggedList;

        private readonly Func<MailMessage, bool> _deleteMessageFromList;

        private readonly Func<Visibility, MailMessage, bool> _changeSendMessageControlsVisibilityAndMessage;

        private Func<Account, IPostService> _getService;

        private Func<Account> _getAccount;

        public ControlMessageViewModel(Func<Account, IPostService> getService, Func<Account> getAccount, Func<MailMessage, Task<bool>> updateFlaggedList, Func<MailMessage, bool> deleteMessageFromList, Func<Visibility, MailMessage, bool> changeSendMessageControlsVisibilityAndMessage)
        {
            _getService = getService;
            _getAccount = getAccount;
            _updateFlaggedList = updateFlaggedList;
            _deleteMessageFromList = deleteMessageFromList;
            _changeSendMessageControlsVisibilityAndMessage = changeSendMessageControlsVisibilityAndMessage;

            FlagMessageCommand = new RelayCommand(FlagMessage);
            DeleteMessageCommand = new RelayCommand(DeleteMessage);
            CloseMessageCommand = new RelayCommand(CloseMessage);
            HideMessageViewCommand = new RelayCommand(HideMessageView);
        }

        #region Flag message
        private async void FlagMessage(object parameter)
        {
            await _getService(_getAccount()).FlagMessage(SelectedMailMessage);
            await _updateFlaggedList(SelectedMailMessage);
        }
        #endregion

        #region Delete message
        private async void DeleteMessage(object parameter)
        {
            await _getService(_getAccount()).DeleteMessage(SelectedMailMessage);
            _deleteMessageFromList(SelectedMailMessage);
            CloseMessage(parameter);
        }
        #endregion

        #region Closing message
        private void CloseMessage(object parameter)
        {
            _selectedMailMessage = new MailMessage();

            MessageViewConrtolVisibility = Visibility.Collapsed;
            _changeSendMessageControlsVisibilityAndMessage(Visibility.Collapsed, _selectedMailMessage);
        }
        #endregion

        #region Hiding message view
        private void HideMessageView(object parameter) => MessageViewConrtolVisibility = Visibility.Collapsed;
        #endregion
    }
}
