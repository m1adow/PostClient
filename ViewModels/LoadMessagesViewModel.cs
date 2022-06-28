using MimeKit;
using PostClient.Models;
using PostClient.Models.Services;
using PostClient.ViewModels.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace PostClient.ViewModels
{
    internal sealed class LoadMessagesViewModel : ViewModelBase
    {
        public ObservableCollection<MailMessage> Messages { get; private set; } = new ObservableCollection<MailMessage>();

        private List<MailMessage> _messages = new List<MailMessage>();

        private int[] _countOfMessages = new int[2] { 0, 10 };

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

        public ICommand LoadMessagesFromLocalStorageCommand { get; }

        public ICommand LoadMessagesFromServerCommand { get; }

        public ICommand LoadNextListOfMessagesCommand { get; }

        public ICommand LoadPreviousListOfMessagesCommand { get; }

        public ICommand CloseMessageCommand { get; }

        public Action LoadMessagesFromServerAction { get; }

        public Action LoadMessagesFromLocalStorageAction { get; }

        private readonly Func<Account> _getAccount;

        public LoadMessagesViewModel(Func<Account> getAccount)
        {
            _getAccount = getAccount;
            LoadMessagesFromServerAction = LoadMessagesFromServer;
            LoadMessagesFromLocalStorageAction = LoadMessagesFromLocalStorage;

            LoadMessagesFromLocalStorageCommand = new RelayCommand(LoadMessagesFromLocalStorage);
            LoadMessagesFromServerCommand = new RelayCommand(LoadMessagesFromServer);
            LoadNextListOfMessagesCommand = new RelayCommand(LoadNextListOfMessages);
            LoadPreviousListOfMessagesCommand = new RelayCommand(LoadPreviousListOfMessages);
            CloseMessageCommand = new RelayCommand(CloseMessage);
        }

        #region Method for load messages from local storage
        private async void LoadMessagesFromLocalStorage()
        {
            _messages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("Messages.json");
            AddMessagesToCollection(_messages);
        }
        #endregion

        #region Method for load messages from server
        private void LoadMessagesFromServer()
        {
            Account account = _getAccount();
            List<MimeMessage> mimeMessages = new List<MimeMessage>();

            switch (account.PostServiceName)
            {
                case nameof(GmailService):
                    mimeMessages = new GmailService().LoadMessages(account, MessageDialogShower.ShowMessageDialog);
                    break;
                case nameof(OutlookService):
                    mimeMessages = new OutlookService().LoadMessages(account, MessageDialogShower.ShowMessageDialog);
                    break;
            }

            var mailMessages = ConvertFromMimeMessageToMailMessage(mimeMessages);
            _messages = mailMessages;
            SaveMessages(mailMessages);
            AddMessagesToCollection(mailMessages);
        }

        private List<MailMessage> ConvertFromMimeMessageToMailMessage(List<MimeMessage> mimeMessages)
        {
            List<MailMessage> mailMessages = new List<MailMessage>();

            foreach (var mimeMessage in mimeMessages)
                mailMessages.Add(CreateMessage(mimeMessage));

            return mailMessages;
        }

        private MailMessage CreateMessage(MimeMessage messageMime)
        {
            MailMessage message = new MailMessage()
            {
                Subject = messageMime.Subject,
                Body = messageMime.HtmlBody,
                From = messageMime.From[0].Name,
                Date = messageMime.Date
            };

            return message;
        }

        private void SaveMessages(List<MailMessage> messages) => JSONSaverAndReaderHelper.Save(messages, "Messages.json");

        private void AddMessagesToCollection(List<MailMessage> messages)
        {
            Messages.Clear();

            int indexOfLastMessage = messages.Count - _countOfMessages[0];
            int indexOfFirstMessage = messages.Count < _countOfMessages[1] ? 0 : messages.Count - _countOfMessages[1];

            try
            {
                CheckForOutOfBounds(indexOfLastMessage, messages.Count, indexOfFirstMessage);

                for (int i = indexOfLastMessage - 1; i > indexOfFirstMessage; i--)
                    Messages.Add(messages[i]);
            }
            catch (Exception exception)
            {
                MessageDialogShower.ShowMessageDialog(exception.Message);
            }
        }

        private void CheckForOutOfBounds(int last, int max, int first)
        {
            if (last > max)
                throw new ArgumentOutOfRangeException("You've reached last list of messages");
            if (first < 0)
                throw new ArgumentOutOfRangeException("You've reached first list of messages");
        }
        #endregion

        #region Method for load next list of messages
        private void LoadNextListOfMessages()
        {
            _countOfMessages = new int[] { _countOfMessages[0] + 10, _countOfMessages[1] + 10 };
            AddMessagesToCollection(_messages);
        }
        #endregion

        #region Method for load previous list of messages
        private void LoadPreviousListOfMessages()
        {
            _countOfMessages = new int[] { _countOfMessages[0] - 10, _countOfMessages[1] - 10 };
            AddMessagesToCollection(_messages);
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
