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

        private List<MimeMessage> _mimeMessages = new List<MimeMessage>();

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

        public ICommand LoadCommand { get; }

        public ICommand LoadNextListOfMessagesCommand { get; }

        public ICommand LoadPreviousListOfMessagesCommand { get; }

        public ICommand CloseMessageCommand { get; }

        public Action LoadMessagesAction { get; }

        private readonly Func<Account> _getAccount;

        public LoadMessagesViewModel(Func<Account> getAccount)
        {
            _getAccount = getAccount;
            LoadMessagesAction = LoadMessages;

            LoadCommand = new RelayCommand(LoadMessages);
            LoadNextListOfMessagesCommand = new RelayCommand(LoadNextListOfMessages);
            LoadPreviousListOfMessagesCommand = new RelayCommand(LoadPreviousListOfMessages);
            CloseMessageCommand = new RelayCommand(CloseMessage);
        }

        #region Method for load messages
        private void LoadMessages()
        {
            Account account = _getAccount();

            switch (account.PostServiceName)
            {
                case nameof(GmailService):
                    AddMessagesToCollection(new GmailService().LoadMessages(account, MessageDialogShower.ShowMessageDialog));
                    break;
                case nameof(OutlookService):
                    AddMessagesToCollection(new OutlookService().LoadMessages(account, MessageDialogShower.ShowMessageDialog));
                    break;
            }
        }

        private void AddMessagesToCollection(List<MimeMessage> messages)
        {
            Messages.Clear();

            if (_mimeMessages != messages)
                _mimeMessages = messages;

            int indexOfLastMessage = messages.Count - _countOfMessages[0];
            int indexOfFirstMessage = messages.Count < _countOfMessages[1] ? 0 : messages.Count - _countOfMessages[1];

            try
            {
                CheckForOutOfBounds(indexOfLastMessage, messages.Count, indexOfFirstMessage);

                for (int i = indexOfLastMessage - 1; i > indexOfFirstMessage - 1; i--)
                {
                    MailMessage mailMessage = CreateMessage(messages[i]);
                    Messages.Add(mailMessage);
                }
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
        #endregion

        #region Method for load next list of messages
        private void LoadNextListOfMessages()
        {
            _countOfMessages = new int[] { _countOfMessages[0] + 10, _countOfMessages[1] + 10 };
            AddMessagesToCollection(_mimeMessages);
        }
        #endregion

        #region Method for load previous list of messages
        private void LoadPreviousListOfMessages()
        {
            _countOfMessages = new int[] { _countOfMessages[0] - 10, _countOfMessages[1] - 10 };
            AddMessagesToCollection(_mimeMessages);
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
