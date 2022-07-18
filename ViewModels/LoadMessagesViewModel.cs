using MailKit;
using MailKit.Search;
using Microsoft.Toolkit.Uwp.Notifications;
using MimeKit;
using PostClient.Models;
using PostClient.Models.Infrastructure;
using PostClient.Models.Services;
using PostClient.ViewModels.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace PostClient.ViewModels
{
    #nullable enable

    internal sealed class LoadMessagesViewModel : ViewModelBase
    {
        private ObservableCollection<MailMessage>? _messages = new ObservableCollection<MailMessage>();

        public ObservableCollection<MailMessage>? Messages
        {
            get => _messages;
            set => Set(ref _messages, value);
        }

        private string? _searchText = string.Empty;

        public string? SearchText
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

        private string _messageFolder = string.Empty;

        private DispatcherTimer? _dispatcherTimer;

        private Func<Account, IPostService> _getService;

        private Func<Account> _getAccount;

        public LoadMessagesViewModel(Func<Account, IPostService> getService, Func<Account> getAccount)
        {
            _getService = getService;
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
            _getAccount = getAccount;
        }

        #region Timer
        private void LaunchTimer()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += Timer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 10, 0);
            _dispatcherTimer?.Start();
        }

        private void Timer_Tick(object sender, object e) => LoadMessagesFromServer(new object());
        #endregion

        #region Load messages from local storage
        private async void LoadMessagesFromLocalStorage(object parameter)
        {
            _messageFolder = parameter.ToString() + ".json";
            Messages = new ObservableCollection<MailMessage>(await JSONSaverAndReaderHelper.Read<List<MailMessage>>(_messageFolder));
        }
        #endregion   

        #region Load messages from server
        private async void LoadMessagesFromServer(object parameter)
        {
            Storyboard? storyboard = (parameter as Storyboard) ?? new Storyboard();

            storyboard.Begin();
            storyboard.RepeatBehavior = RepeatBehavior.Forever;

            try
            {
                Messages = await GetMessagesAsync();
            }
            catch (MailKit.Security.AuthenticationException exception)
            {
                MessageDialogShower.ShowMessageDialog(exception.Message);
            }

            storyboard.Stop();
        }

        private async Task<ObservableCollection<MailMessage>> GetMessagesAsync()
        {
            var messages = new ObservableCollection<MailMessage>();
            var tempMessages = Messages;
            Messages?.Clear();

            await Task.Run(() =>
            {
                var allMimeMessages = GetMimeMessagesAsync(SpecialFolder.All, SearchQuery.All);
                var flaggedMimeMessages = GetMimeMessagesAsync(SpecialFolder.All, SearchQuery.Flagged);
                var sentMimeMessages = GetMimeMessagesAsync(SpecialFolder.Sent, SearchQuery.All);

                var allMailMessages = ConvertFromMimeMessageToMailMessage(allMimeMessages);
                SendNotificationsAboutNewMessages(allMailMessages);
                messages = new ObservableCollection<MailMessage>(allMailMessages);

                var sentMailMessages = ConvertFromMimeMessageToMailMessage(sentMimeMessages);

                var flaggedMailMessages = ConvertFromMimeMessageToMailMessage(flaggedMimeMessages);
                flaggedMailMessages.ForEach(m => m.IsFlagged = true);

                SaveMessages(allMailMessages, "AllMessages.json");
                SaveMessages(sentMailMessages, "SentMessages.json");
                SaveMessages(flaggedMailMessages, "FlaggedMessages.json");
                SaveMessages(new List<MailMessage>(), "DraftMessages.json"); //clear draft messages after syncing
            });

            return messages;
        }

        private Dictionary<UniqueId, MimeMessage> GetMimeMessagesAsync(SpecialFolder specialFolder, SearchQuery searchQuery) => _getService(_getAccount()).LoadMessages(specialFolder, searchQuery);

        private List<MailMessage> ConvertFromMimeMessageToMailMessage(Dictionary<UniqueId, MimeMessage> mimeMessages)
        {
            var mailMessages = new List<MailMessage>();

            foreach (var mimeMessage in mimeMessages)
                mailMessages?.Add(CreateMessage(mimeMessage));

            return mailMessages;
        }

        private MailMessage CreateMessage(KeyValuePair<UniqueId, MimeMessage> mimeMessage)
        {
            var message = new MailMessage
            {
                Uid = mimeMessage.Key.Id,
                Subject = mimeMessage.Value.Subject,
                Body = mimeMessage.Value.HtmlBody ?? "",
                Attachments = ConvertMimeAttachmentsToMailMessageAttachments(mimeMessage.Value.Attachments),
                From = mimeMessage.Value.From[0].Name,
                Date = mimeMessage.Value.Date
            };

            return message;
        }

        private List<KeyValuePair<string, byte[]>> ConvertMimeAttachmentsToMailMessageAttachments(IEnumerable<MimeEntity> attachments)
        {
            var mailAttachments = new List<KeyValuePair<string, byte[]>>();

            foreach (var attachment in attachments)
            {
                string fileName = attachment.ContentDisposition.FileName;               

                using (var memoryStream = new MemoryStream())
                {
                    if (attachment is MimePart part)
                        part.Content.DecodeTo(memoryStream);
                    else
                        ((MessagePart)attachment).Message.WriteTo(memoryStream);

                    byte[] file = memoryStream.ToArray();

                    mailAttachments?.Add(new KeyValuePair<string, byte[]>(fileName, file));
                }
            }

            return mailAttachments;
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

        #region Flag message
        private async Task<bool> FlagMessage(MailMessage message)
        {
            var flaggedMessages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("FlaggedMessages.json");

            var flagMessage = flaggedMessages.Where(m => m.Equals(message)).FirstOrDefault() ?? message;

            if (flagMessage.IsFlagged)
            {
                flagMessage.IsFlagged = false;
                flaggedMessages?.Remove(flagMessage);
            }
            else
            {
                flagMessage.IsFlagged = true;
                flaggedMessages?.Add(flagMessage);
            }

            JSONSaverAndReaderHelper.Save(flaggedMessages, "FlaggedMessages.json");

            return true;
        }
        #endregion

        #region Delete message
        private bool DeleteMessage(MailMessage message)
        {
            Messages?.Remove(message);
            JSONSaverAndReaderHelper.Save(Messages, _messageFolder);
            return true;
        }
        #endregion

        #region Search message
        private async void SearchMessage(object parameter) => Messages = new ObservableCollection<MailMessage>((await JSONSaverAndReaderHelper.Read<List<MailMessage>>(_messageFolder)).Where(m => m.Subject.ToLower().Contains(_searchText.ToLower()) || m.From.ToLower().Contains(_searchText.ToLower())));
        #endregion

        #region Sort messages 
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
