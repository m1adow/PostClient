using MimeKit;
using PostClient.Models;
using PostClient.Models.Services;
using PostClient.ViewModels.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;

namespace PostClient.ViewModels
{
    internal sealed class SendMessageViewModel : ViewModelBase
    {
        private Account _account = new Account();

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

        public ICommand SendMessageCommand { get; private set; }

        public SendMessageViewModel()
        {
            SendMessageCommand = new RelayCommand(SendMessage, IsFieldsFilled);

            GetAccountAndFillEmail();
        }

        private async void GetAccountAndFillEmail()
        {
            _account = await JSONSaverAndReaderHelper.Read();
            MessageSender = _account.Email;
        }

        #region Methods for sending message
        private void SendMessage()
        {
            switch (_account.PostServiceName)
            {
                case nameof(GmailService):
                    new GmailService().SendMessage(_account, CreateMessage());
                    break;
                case nameof(OutlookService):
                    new OutlookService().SendMessage(_account, CreateMessage());
                    break;
            }
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

        private async void ShowMessageDialogForException(Exception exception)
        {
            MessageDialog messageDialog = new MessageDialog(exception.Message);
            await messageDialog.ShowAsync();
        }

        private bool IsFieldsFilled() => MessageReciever.Length > 0;
        #endregion
    }
}
