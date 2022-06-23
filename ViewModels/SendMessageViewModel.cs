using MailKit.Net.Smtp;
using MimeKit;
using PostClient.Models;
using PostClient.ViewModels.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Windows.Input;
using Windows.UI.Popups;

namespace PostClient.ViewModels
{
    public class SendMessageViewModel : ViewModelBase
    {
        private readonly Account _account = new Account();

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

            _account = JSONSaverAndReaderHelper.Read().Result;
            MessageSender = _account.Email;
        }

        #region Methods for sending message
        private async void SendMessage()
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(MessageName, _account.Email));
            message.To.Add(MailboxAddress.Parse(MessageReciever));
            message.Subject = MessageSubject;
            message.Body = new TextPart()
            {
                Text = MessageBody
            };

            SmtpClient client = new SmtpClient();

            try
            {
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate(_account.Email, _account.Password);
                client.Send(message);
            }
            catch (Exception exc)
            {
                MessageDialog messageDialog = new MessageDialog(exc.Message);
                await messageDialog.ShowAsync();
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

        private bool IsFieldsFilled() => MessageReciever.Length > 0;
        #endregion
    }
}
