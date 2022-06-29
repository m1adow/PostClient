using MailKit;
using MailKit.Search;
using MimeKit;
using PostClient.Models;
using PostClient.Models.Services;
using PostClient.ViewModels.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PostClient.ViewModels
{
    internal sealed class LoadMessagesViewModel : ViewModelBase
    {
        public ObservableCollection<MailMessage> Messages { get; } = new ObservableCollection<MailMessage>();

        private List<MailMessage> _allMessages = new List<MailMessage>();

        private List<MailMessage> _deletedMessages = new List<MailMessage>();

        private List<MailMessage> _flaggedMessages = new List<MailMessage>();

        private List<MailMessage> _draftMessages = new List<MailMessage>();


        public ICommand LoadAllMessagesFromLocalStorageCommand { get; }

        public ICommand LoadDeletedMessagesFromLocalStorageCommand { get; }

        public ICommand LoadFlaggedMessagesFromLocalStorageCommand { get; }

        public ICommand LoadDraftMessagesFromLocalStorageCommand { get; }

        public ICommand LoadAllMessagesFromServerCommand { get; }

        public Action LoadMessagesFromServerAction { get; }

        public Action LoadMessagesFromLocalStorageAction { get; }

        public Func<MailMessage, bool> FlagMessageFunc { get; }

        public Func<MailMessage, bool> DeleteMessageFunc { get; }

        private readonly Func<Account> _getAccount;

        public LoadMessagesViewModel(Func<Account> getAccount)
        {
            _getAccount = getAccount;
            LoadMessagesFromServerAction = LoadAllMessagesFromServer;
            LoadMessagesFromLocalStorageAction = LoadAllMessagesFromLocalStorage;
            FlagMessageFunc = FlagMessage;
            DeleteMessageFunc = DeleteMessage;

            LoadAllMessagesFromLocalStorageCommand = new RelayCommand(LoadAllMessagesFromLocalStorage);
            LoadDeletedMessagesFromLocalStorageCommand = new RelayCommand(LoadDeletedMessagesFromLocalStorage);
            LoadFlaggedMessagesFromLocalStorageCommand = new RelayCommand(LoadFlaggedMessagesFromLocalStorage);
            LoadDraftMessagesFromLocalStorageCommand = new RelayCommand(LoadDraftMessagesFromLocalStorage);
            LoadAllMessagesFromServerCommand = new RelayCommand(LoadAllMessagesFromServer);
        }

        #region Method for load messages from local storage
        private async void LoadAllMessagesFromLocalStorage()
        {
            _allMessages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("AllMessages.json");
            _flaggedMessages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("FlaggedMessages.json");
            _deletedMessages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("DeletedMessages.json");
            _draftMessages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("DraftMessages.json");
            UpdateMessageCollection(_allMessages);
        }
        #endregion

        #region Method for load deleted messages
        private void LoadDeletedMessagesFromLocalStorage() => UpdateMessageCollection(_deletedMessages);
        #endregion

        #region Method for load flagged messages
        private void LoadFlaggedMessagesFromLocalStorage() => UpdateMessageCollection(_flaggedMessages);
        #endregion

        #region Method for load draft messages
        private void LoadDraftMessagesFromLocalStorage() => UpdateMessageCollection(_draftMessages);
        #endregion

        #region Method for load messages from server
        private void LoadAllMessagesFromServer()
        {
            Account account = _getAccount();

            var allMimeMessages = GetMimeMessages(account, SearchQuery.All);
            var deletedMimeMessages = GetMimeMessages(account, SearchQuery.Deleted);
            var flaggedMimeMessages = GetMimeMessages(account, SearchQuery.Flagged);
            var draftMimeMessages = GetMimeMessages(account, SearchQuery.Draft);

            var allMailMessages = ConvertFromMimeMessageToMailMessage(allMimeMessages);
            _allMessages = allMailMessages;

            var deletedMailMessages = ConvertFromMimeMessageToMailMessage(deletedMimeMessages);
            _deletedMessages = deletedMailMessages;

            var flaggedMailMessages = ConvertFromMimeMessageToMailMessage(flaggedMimeMessages);
            flaggedMailMessages.ForEach(m => m.IsFlagged = true);
            _flaggedMessages = flaggedMailMessages;

            var draftMailMessages = ConvertFromMimeMessageToMailMessage(draftMimeMessages);
            _draftMessages = draftMailMessages;

            SaveMessages(allMailMessages, "AllMessages.json");
            SaveMessages(deletedMailMessages, "DeletedMessages.json");
            SaveMessages(flaggedMailMessages, "FlaggedMessages.json");
            SaveMessages(draftMailMessages, "DraftMessages.json");

            UpdateMessageCollection(_allMessages);
        }

        private Dictionary<UniqueId, MimeMessage> GetMimeMessages(Account account, SearchQuery searchQuery)
        {
            Dictionary<UniqueId, MimeMessage> mimeMessages = new Dictionary<UniqueId, MimeMessage>();

            switch (account.PostServiceName)
            {
                case nameof(GmailService):
                    mimeMessages = new GmailService().LoadMessages(account, searchQuery, MessageDialogShower.ShowMessageDialog);
                    break;
                case nameof(OutlookService):
                    mimeMessages = new OutlookService().LoadMessages(account, searchQuery, MessageDialogShower.ShowMessageDialog);
                    break;
            }

            return mimeMessages;
        }

        private List<MailMessage> ConvertFromMimeMessageToMailMessage(Dictionary<UniqueId, MimeMessage> mimeMessages)
        {
            List<MailMessage> mailMessages = new List<MailMessage>();

            foreach (var mimeMessage in mimeMessages)
                mailMessages.Add(CreateMessage(mimeMessage));

            return mailMessages;
        }

        private MailMessage CreateMessage(KeyValuePair<UniqueId, MimeMessage> mimeMessage)
        {
            MailMessage message = new MailMessage()
            {
                Uid = mimeMessage.Key.Id,
                Subject = mimeMessage.Value.Subject,
                Body = mimeMessage.Value.HtmlBody,
                From = mimeMessage.Value.From[0].Name,
                Date = mimeMessage.Value.Date
            };

            return message;
        }

        private void SaveMessages(List<MailMessage> messages, string name) => JSONSaverAndReaderHelper.Save(messages, name);

        private void UpdateMessageCollection(List<MailMessage> messages)
        {
            Messages.Clear();

            for (int i = 0; i < messages.Count; i++)
                Messages.Add(_allMessages[i]);
        }
        #endregion

        private bool FlagMessage(MailMessage message)
        {
            if (message.IsFlagged)
            {
                message.IsFlagged = false;
                _flaggedMessages.Remove(message);
            }
            else
            {
                message.IsFlagged = true;
                _flaggedMessages.Add(message);
            }

            JSONSaverAndReaderHelper.Save(_flaggedMessages, "FlaggedMessages.json");

            return true;
        }

        private bool DeleteMessage(MailMessage message)
        {
            _allMessages.Remove(message);

            JSONSaverAndReaderHelper.Save(_allMessages, "AllMessages.json");

            UpdateMessageCollection(_allMessages);

            return true;
        }
    }
}
