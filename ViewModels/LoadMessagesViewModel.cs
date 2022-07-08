using MailKit;
using MailKit.Search;
using Microsoft.Toolkit.Uwp.Notifications;
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
using Windows.UI.Xaml;

namespace PostClient.ViewModels
{
    internal sealed class LoadMessagesViewModel : ViewModelBase
    {
        private ObservableCollection<MailMessage> _messages = new ObservableCollection<MailMessage>();

        public ObservableCollection<MailMessage> Messages
        {
            get => _messages;
            set => Set(ref _messages, value);
        }

        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value);
        }

        public ICommand LoadMessagesFromLocalStorageCommand { get; }

        public ICommand LoadMessagesFromServerCommand { get; }

        public ICommand SearchMessageCommand { get; }

        public ICommand SortMessagesCommand { get; }

        public Action<object> LoadMessagesFromServerAction { get; }

        public Action<object> LoadMessagesFromLocalStorageAction { get; }

        public Func<MailMessage, Task<bool>> FlagMessageFunc { get; }

        public Func<MailMessage, bool> DeleteMessageFunc { get; }

        private readonly Func<Account> _getAccount;

        private string _messageFolder = string.Empty;

        private DispatcherTimer _dispatcherTimer;

        public LoadMessagesViewModel(Func<Account> getAccount)
        {
            _getAccount = getAccount;
            LoadMessagesFromServerAction = LoadMessagesFromServer;
            LoadMessagesFromLocalStorageAction = LoadMessagesFromLocalStorage;
            FlagMessageFunc = FlagMessage;
            DeleteMessageFunc = DeleteMessage;

            LoadMessagesFromLocalStorageCommand = new RelayCommand(LoadMessagesFromLocalStorage);         
            LoadMessagesFromServerCommand = new RelayCommand(LoadMessagesFromServer);
            SearchMessageCommand = new RelayCommand(SearchMessage);
            SortMessagesCommand = new RelayCommand(SortMessages);

            LaunchTimer();
        }

        #region Timer
        private void LaunchTimer()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += Timer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 10, 0);
            _dispatcherTimer.Start();
        }

        private void Timer_Tick(object sender, object e) => LoadMessagesFromServer(new object());
        #endregion

        #region Method for load messages from local storage
        private async void LoadMessagesFromLocalStorage(object parameter)
        {
            _messageFolder = parameter.ToString() + ".json";
            Messages = new ObservableCollection<MailMessage>(await JSONSaverAndReaderHelper.Read<List<MailMessage>>(_messageFolder));
        }
        #endregion   

        #region Method for load messages from server
        private async void LoadMessagesFromServer(object parameter) => await GetMessagesAsync();

        private async Task GetMessagesAsync()
        {
            await Task.Run(() =>
            {
                Account account = _getAccount();

                var allMimeMessages = GetMimeMessages(account, SpecialFolder.All, SearchQuery.All);
                var flaggedMimeMessages = GetMimeMessages(account, SpecialFolder.All, SearchQuery.Flagged);
                var sentMimeMessages = GetMimeMessages(account, SpecialFolder.Sent, SearchQuery.All); ;

                var allMailMessages = ConvertFromMimeMessageToMailMessage(allMimeMessages);
                SendNotificationsAboutNewMessages(allMailMessages);
                Messages = new ObservableCollection<MailMessage>(allMailMessages);

                var sentMailMessages = ConvertFromMimeMessageToMailMessage(sentMimeMessages);

                var flaggedMailMessages = ConvertFromMimeMessageToMailMessage(flaggedMimeMessages);
                flaggedMailMessages.ForEach(m => m.IsFlagged = true);

                SaveMessages(allMailMessages, "AllMessages.json");
                SaveMessages(sentMailMessages, "SentMessages.json");
                SaveMessages(flaggedMailMessages, "FlaggedMessages.json");
                SaveMessages(new List<MailMessage>(), "DraftMessages.json"); //clear draft messages after syncing
            });
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

        private void SendNotificationsAboutNewMessages(List<MailMessage> messages)
        {
            foreach (var message in messages)
            {
                if (!Messages.Contains(message))
                {
                    new ToastContentBuilder()
                        .AddArgument("action", "viewConversation")
                        .AddText($"{message.From} sent you a message")
                        .AddText($"Check this out, {message.Subject}")
                        .Show();
                }
            }
        }

        private void SaveMessages(List<MailMessage> messages, string name) => JSONSaverAndReaderHelper.Save(messages, name);
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
            Messages.Remove(message);
            JSONSaverAndReaderHelper.Save(Messages, _messageFolder);
            return true;
        }
        #endregion

        #region Method for search message
        private async void SearchMessage(object parameter) => Messages = new ObservableCollection<MailMessage>((await JSONSaverAndReaderHelper.Read<List<MailMessage>>(_messageFolder)).Where(m => m.Subject.ToLower().Contains(_searchText.ToLower()) || m.From.ToLower().Contains(_searchText.ToLower())));
        #endregion

        #region Methods for sort messages 
        private void SortMessages(object parameter)
        {
            switch (parameter.ToString())
            {
                case "newer":
                    Messages = new ObservableCollection<MailMessage>(Messages.OrderByDescending(m => m.Date));
                    break;
                case "older":
                    Messages = new ObservableCollection<MailMessage>(Messages.OrderBy(m => m.Date));
                    break;
                case "a-z":
                    Messages = new ObservableCollection<MailMessage>(Messages.OrderBy(m => m.Subject));
                    break;
                case "z-a":
                    Messages = new ObservableCollection<MailMessage>(Messages.OrderByDescending(m => m.Subject));
                    break;
            }
        }
        #endregion
    }
}
