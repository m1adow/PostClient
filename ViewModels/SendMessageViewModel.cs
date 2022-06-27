using MimeKit;
using PostClient.Models;
using PostClient.Models.Services;
using PostClient.ViewModels.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace PostClient.ViewModels
{
    internal sealed class SendMessageViewModel : ViewModelBase
    {
        private Visibility _sendMessageControlsVisibility = Visibility.Collapsed;

        public Visibility SendMessageControlsVisibility
        {
            get => _sendMessageControlsVisibility;
            set => Set(ref _sendMessageControlsVisibility, value);
        }

        private string _messageSender = string.Empty;

        public string MessageSender
        {
            get => _messageSender;
            private set => Set(ref _messageSender, value);
        }

        private string _messageReciever = string.Empty;

        public string MessageReciever
        {
            get => _messageReciever;
            set => Set(ref _messageReciever, value, new ICommand[] { SendMessageCommand });
        }

        private string _messageName = "New message";

        public string MessageName
        {
            get => _messageName;
            set => Set(ref _messageName, value);
        }

        private string _messageSubject = "It's my beautiful post app";

        public string MessageSubject
        {
            get => _messageSubject;
            set => Set(ref _messageSubject, value);
        }

        private string _messageBody = "Hi world!";

        public string MessageBody
        {
            get => _messageBody;
            set => Set(ref _messageBody, value);
        }

        public ICommand SendMessageCommand { get; }

        public ICommand CancelSendingMessageCommand { get; }

        public ICommand ShowSendingControlosCommand { get; }

        private Account _account = new Account();

        private Func<Account> _getAccount;

        public SendMessageViewModel(Func<Account> getAccount)
        {
            _getAccount = getAccount;
            _account = getAccount();

            SendMessageCommand = new RelayCommand(SendMessage, IsSendMessageFieldsFilled);
            CancelSendingMessageCommand = new RelayCommand(CancelSendingMessage);
            ShowSendingControlosCommand = new RelayCommand(ShowSendMessageControlsAndLoadAccount);
        }

        #region Methods for sending message
        private void SendMessage()
        {
            switch (_account.PostServiceName)
            {
                case nameof(GmailService):
                    new GmailService().SendMessage(_account, CreateMessage(), MessageDialogShower.ShowMessageDialog);
                    break;
                case nameof(OutlookService):
                    new OutlookService().SendMessage(_account, CreateMessage(), MessageDialogShower.ShowMessageDialog);
                    break;
            }

            MessageDialogShower.ShowMessageDialog("Mail has sent successfully");
            ClearFields();
            SendMessageControlsVisibility = Visibility.Collapsed;
        }

        private MimeMessage CreateMessage()
        {
            MimeMessage message = new MimeMessage();

            message.From.Add(new MailboxAddress(MessageName, _account.Email));
            message.To.Add(MailboxAddress.Parse(MessageReciever));
            message.Subject = MessageSubject;
            message.Body = new TextPart()
            {
                Text = MessageBody
            };

            return message;
        }

        private void ClearFields()
        {
            MessageReciever = string.Empty;
            MessageName = "New message";
            MessageSubject = "It's my beautiful post app";
            MessageBody = "Hi world!";
        }

        private bool IsSendMessageFieldsFilled() => MessageReciever.Length > 0;
        #endregion

        #region Methods for cancel sending 
        private void CancelSendingMessage() => SendMessageControlsVisibility = Visibility.Collapsed;
        #endregion

        #region Method for send command
        private void ShowSendMessageControlsAndLoadAccount()
        {
            SendMessageControlsVisibility = Visibility.Visible;

            _account = _getAccount();
            MessageSender = _account.Email;
        }
        #endregion
    }
}
