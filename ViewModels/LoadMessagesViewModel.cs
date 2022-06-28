using MailKit.Search;
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

        public ICommand LoadAllMessagesFromLocalStorageCommand { get; }

        public ICommand LoadDeletedMessagesFromLocalStorageCommand { get; }

        public ICommand LoadFlaggedMessagesFromLocalStorageCommand { get; }

        public ICommand LoadDraftMessagesFromLocalStorageCommand { get; }

        public ICommand LoadAllMessagesFromServerCommand { get; }

        public Action LoadMessagesFromServerAction { get; }

        public Action LoadMessagesFromLocalStorageAction { get; }

        private readonly Func<Account> _getAccount;

        public LoadMessagesViewModel(Func<Account> getAccount)
        {
            _getAccount = getAccount;
            LoadMessagesFromServerAction = LoadAllMessagesFromServer;
            LoadMessagesFromLocalStorageAction = LoadAllMessagesFromLocalStorage;

            LoadAllMessagesFromLocalStorageCommand = new RelayCommand(LoadAllMessagesFromLocalStorage);
            LoadDeletedMessagesFromLocalStorageCommand = new RelayCommand(LoadDeletedMessagesFromLocalStorage);
            LoadFlaggedMessagesFromLocalStorageCommand = new RelayCommand(LoadFlaggedMessagesFromLocalStorage);
            LoadDraftMessagesFromLocalStorageCommand = new RelayCommand(LoadDraftMessagesFromLocalStorage);
            LoadAllMessagesFromServerCommand = new RelayCommand(LoadAllMessagesFromServer);                   
        }

        #region Method for load messages from local storage
        private async void LoadAllMessagesFromLocalStorage()
        {
            _messages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("AllMessages.json");
            AddMessagesToCollection(_messages);
        }
        #endregion

        #region Method for load deleted messages
        private async void LoadDeletedMessagesFromLocalStorage()
        {
            _messages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("DeletedMessages.json"); ;
            AddMessagesToCollection(_messages);
        }
        #endregion

        #region Method for load flagged messages
        private async void LoadFlaggedMessagesFromLocalStorage()
        {
            _messages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("FlaggedMessages.json");
            AddMessagesToCollection(_messages);
        }
        #endregion

        #region Method for load draft messages
        private async void LoadDraftMessagesFromLocalStorage()
        {
            _messages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("DraftMessages.json");
            AddMessagesToCollection(_messages);
        }
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
            _messages = allMailMessages;

            var deletedMailMessages = ConvertFromMimeMessageToMailMessage(deletedMimeMessages);
            var flaggedMailMessages = ConvertFromMimeMessageToMailMessage(flaggedMimeMessages);
            var draftMailMessages = ConvertFromMimeMessageToMailMessage(draftMimeMessages);

            SaveMessages(allMailMessages, "AllMessages.json");
            SaveMessages(deletedMailMessages, "DeletedMessages.json");
            SaveMessages(flaggedMailMessages, "FlaggedMessages.json");
            SaveMessages(draftMailMessages, "DraftMessages.json");

            AddMessagesToCollection(allMailMessages);
        }

        private List<MimeMessage> GetMimeMessages(Account account, SearchQuery searchQuery)
        {
            List<MimeMessage> mimeMessages = new List<MimeMessage>();

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

        private void SaveMessages(List<MailMessage> messages, string name) => JSONSaverAndReaderHelper.Save(messages, name);

        private void AddMessagesToCollection(List<MailMessage> messages)
        {
            Messages.Clear();

            for (int i = messages.Count - 1; i >= 0; i--)
                Messages.Add(messages[i]);
        }
        #endregion
    }
}
