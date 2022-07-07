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
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PostClient.ViewModels
{
    internal sealed class LoadMessagesViewModel : ViewModelBase
    {
        public ObservableCollection<MailMessage> Messages { get; } = new ObservableCollection<MailMessage>();

        private List<MailMessage> _messages = new List<MailMessage>();

        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value);
        }

        public ICommand LoadAllMessagesFromLocalStorageCommand { get; }

        public ICommand LoadSentMessagesFromLocalStorageCommand { get; }

        public ICommand LoadFlaggedMessagesFromLocalStorageCommand { get; }

        public ICommand LoadDraftMessagesFromLocalStorageCommand { get; }

        public ICommand LoadAllMessagesFromServerCommand { get; }

        public ICommand SearchMessageCommand { get; }

        public ICommand SortMessagesCommand { get; }

        public Action<object> LoadMessagesFromServerAction { get; }

        public Action<object> LoadMessagesFromLocalStorageAction { get; }

        public Func<MailMessage, Task<bool>> FlagMessageFunc { get; }

        public Func<MailMessage, bool> DeleteMessageFunc { get; }

        private readonly Func<Account> _getAccount;

        private string _messageFolder = string.Empty;

        public LoadMessagesViewModel(Func<Account> getAccount)
        {
            _getAccount = getAccount;
            LoadMessagesFromServerAction = LoadAllMessagesFromServer;
            LoadMessagesFromLocalStorageAction = LoadAllMessagesFromLocalStorage;
            FlagMessageFunc = FlagMessage;
            DeleteMessageFunc = DeleteMessage;

            LoadAllMessagesFromLocalStorageCommand = new RelayCommand(LoadAllMessagesFromLocalStorage);
            LoadSentMessagesFromLocalStorageCommand = new RelayCommand(LoadSentMessagesFromLocalStorage);
            LoadFlaggedMessagesFromLocalStorageCommand = new RelayCommand(LoadFlaggedMessagesFromLocalStorage);
            LoadDraftMessagesFromLocalStorageCommand = new RelayCommand(LoadDraftMessagesFromLocalStorage);
            LoadAllMessagesFromServerCommand = new RelayCommand(LoadAllMessagesFromServer);
            SearchMessageCommand = new RelayCommand(SearchMessage);
            SortMessagesCommand = new RelayCommand(SortMessages);
        }

        #region Method for load messages from local storage
        private async void LoadAllMessagesFromLocalStorage(object parameter)
        {
            string path = "AllMessages.json";

            _messageFolder = path;
            _messages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>(path);
            UpdateMessageCollection();
        }
        #endregion

        #region Method for load sent messages
        private async void LoadSentMessagesFromLocalStorage(object parameter)
        {
            string path = "SentMessages.json";

            _messageFolder = path;
            _messages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>(path);
            UpdateMessageCollection();
        }
        #endregion

        #region Method for load flagged messages
        private async void LoadFlaggedMessagesFromLocalStorage(object parameter)
        {
            string path = "FlaggedMessages.json";

            _messageFolder = path;
            _messages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>(path);
            UpdateMessageCollection();
        }
        #endregion

        #region Method for load draft messages
        private async void LoadDraftMessagesFromLocalStorage(object parameter)
        {
            string path = "DraftMessages.json";

            _messageFolder = path;
            _messages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>(path);
            UpdateMessageCollection();
        }
        #endregion

        #region Method for load messages from server
        private void LoadAllMessagesFromServer(object parameter)
        {
            Account account = _getAccount();

            var allMimeMessages = GetMimeMessages(account, SpecialFolder.All, SearchQuery.All);
            var flaggedMimeMessages = GetMimeMessages(account, SpecialFolder.All, SearchQuery.Flagged);
            var sentMimeMessages = GetMimeMessages(account, SpecialFolder.Sent, SearchQuery.All); ;

            var allMailMessages = ConvertFromMimeMessageToMailMessage(allMimeMessages);
            _messages = allMailMessages;

            var sentMailMessages = ConvertFromMimeMessageToMailMessage(sentMimeMessages);

            var flaggedMailMessages = ConvertFromMimeMessageToMailMessage(flaggedMimeMessages);
            flaggedMailMessages.ForEach(m => m.IsFlagged = true);

            SaveMessages(allMailMessages, "AllMessages.json");
            SaveMessages(sentMailMessages, "SentMessages.json");
            SaveMessages(flaggedMailMessages, "FlaggedMessages.json");
            SaveMessages(new List<MailMessage>(), "DraftMessages.json"); //clear draft messages after syncing

            UpdateMessageCollection();
        }

        private Dictionary<UniqueId, MimeMessage> GetMimeMessages(Account account, SpecialFolder specialFolder, SearchQuery searchQuery)
        {
            Dictionary<UniqueId, MimeMessage> mimeMessages = new Dictionary<UniqueId, MimeMessage>();

            switch (account.PostServiceName)
            {
                case nameof(GmailService):
                    mimeMessages = new GmailService(account).LoadMessages(specialFolder, searchQuery, MessageDialogShower.ShowMessageDialog);
                    break;
                case nameof(OutlookService):
                    mimeMessages = new OutlookService(account).LoadMessages(specialFolder, searchQuery, MessageDialogShower.ShowMessageDialog);
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
                Body = mimeMessage.Value.HtmlBody ?? "",
                From = mimeMessage.Value.From[0].Name,
                Date = mimeMessage.Value.Date
            };

            return message;
        }

        private void SaveMessages(List<MailMessage> messages, string name) => JSONSaverAndReaderHelper.Save(messages, name);

        private void UpdateMessageCollection()
        {
            Messages.Clear();

            for (int i = 0; i < _messages.Count; i++)
                Messages.Add(_messages[i]);
        }
        #endregion

        #region Method for flag message
        private async Task<bool> FlagMessage(MailMessage message)
        {
            List<MailMessage> flaggedMessages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("FlaggedMessages.json");

            var flagMessage = flaggedMessages.Where(m => m.Equals(message)).FirstOrDefault() ?? message;

            if (flagMessage.IsFlagged)
            {
                flagMessage.IsFlagged = false;
                flaggedMessages.Remove(flagMessage);
            }
            else
            {
                flagMessage.IsFlagged = true;
                flaggedMessages.Add(flagMessage);
            }

            JSONSaverAndReaderHelper.Save(flaggedMessages, "FlaggedMessages.json");

            return true;
        }
        #endregion

        #region Method for delete message
        private bool DeleteMessage(MailMessage message)
        {
            _messages.Remove(message);

            JSONSaverAndReaderHelper.Save(_messages, _messageFolder);

            UpdateMessageCollection();

            return true;
        }
        #endregion

        #region Method for search message
        private async void SearchMessage(object parameter)
        {
            _messages = (await JSONSaverAndReaderHelper.Read<List<MailMessage>>(_messageFolder)).Where(m => m.Subject.ToLower().Contains(_searchText.ToLower()) || m.From.ToLower().Contains(_searchText.ToLower())).ToList();
            UpdateMessageCollection();
        }
        #endregion

        #region Methods for sort messages 
        private void SortMessages(object parameter)
        {
            switch (parameter.ToString())
            {
                case "newer":
                    _messages = _messages.OrderByDescending(m => m.Date).ToList();
                    break;
                case "older":
                    _messages = _messages.OrderBy(m => m.Date).ToList();
                    break;
                case "a-z":
                    _messages = _messages.OrderBy(m => m.Subject).ToList();
                    break;
                case "z-a":
                    _messages = _messages.OrderByDescending(m => m.Subject).ToList();
                    break;
            }
            UpdateMessageCollection();
        }
        #endregion
    }
}
