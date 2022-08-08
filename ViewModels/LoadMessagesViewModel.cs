using MailKit;
using MailKit.Search;
using Microsoft.Toolkit.Uwp.Notifications;
using MimeKit;
using PostClient.Models;
using PostClient.Models.Infrastructure;
using PostClient.Models.Helpers;
using PostClient.ViewModels.Infrastructure;
using PostClientBackground;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Background;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Animation;

namespace PostClient.ViewModels
{
#nullable enable

    public class LoadMessagesViewModel : ViewModelBase
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

        public Func<MailMessage, Task> FlagMessageFunc { get; }

        public Func<MailMessage, Task> DeleteMessageFunc { get; }

        public Func<MailMessage, Task> ArchiveMessageAction { get; }

        public Func<MailMessage, MailMessage, Task> UpdateMessagesAction { get; }

        private string _messageFolder = string.Empty;

        private readonly Func<IPostService> _getService;

        private IBackgroundTaskRegistration? _backgroundTask;

        public LoadMessagesViewModel(Func<IPostService> getService)
        {
            _getService = getService;
            LoadMessagesFromServerAction = LoadMessagesFromServer;
            LoadMessagesFromLocalStorageAction = LoadMessagesFromLocalStorage;
            FlagMessageFunc = FlagMessage;
            DeleteMessageFunc = DeleteMessage;
            ArchiveMessageAction = ArchiveMessage;
            UpdateMessagesAction = UpdateMessages;

            LoadMessagesFromLocalStorageCommand = new RelayCommand(LoadMessagesFromLocalStorage);
            LoadMessagesFromServerCommand = new RelayCommand(LoadMessagesFromServer);
            SearchMessageCommand = new RelayCommand(SearchMessage);
            SortMessagesCommand = new RelayCommand(SortMessages);

            //_backgroundTask = UpdatingMessagesBackground.Register();
            //_backgroundTask.Completed += BackgroundTask_Completed;
        }

        private async void BackgroundTask_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    Messages = await GetMessagesAsync();
                });
        }

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
            Storyboard? storyboard = parameter as Storyboard;

            if (storyboard != null)
            {
                storyboard.Begin();
                storyboard.RepeatBehavior = RepeatBehavior.Forever;
            }

            try
            {
                Messages = await GetMessagesAsync();
                _messageFolder = "AllMessages.json";
            }
            catch (Exception exception)
            {
                MessageDialogShower.ShowMessageDialog(exception.Message);
            }

            MessageDialogShower.ShowMessageDialog("Messages was downloaded");
            storyboard?.Stop();
        }

        private async Task<ObservableCollection<MailMessage>> GetMessagesAsync()
        {
            var messages = new ObservableCollection<MailMessage>();
            var tempMessages = Messages;
            Messages?.Clear();

            var allMimeMessages = await GetMimeMessagesAsync(SpecialFolder.All, SearchQuery.All);
            var flaggedMimeMessages = await GetMimeMessagesAsync(SpecialFolder.All, SearchQuery.Flagged);
            var sentMimeMessages = await GetMimeMessagesAsync(SpecialFolder.Sent, SearchQuery.All, "Sent");

            var allMailMessages = ConvertFromMimeMessageToMailMessage(allMimeMessages);
            SendNotificationAboutNewMessages(allMailMessages, tempMessages.ToList());
            messages = new ObservableCollection<MailMessage>(allMailMessages);

            var sentMailMessages = ConvertFromMimeMessageToMailMessage(sentMimeMessages);
            sentMailMessages.ForEach(m => m.Folder = "Sent");

            var flaggedMailMessages = ConvertFromMimeMessageToMailMessage(flaggedMimeMessages);
            flaggedMailMessages.ForEach(m =>
            {
                m.IsFlagged = true;
                m.Folder = "Flagged";
            });

            SaveMessages(allMailMessages, "AllMessages.json");
            SaveMessages(sentMailMessages, "SentMessages.json");
            SaveMessages(flaggedMailMessages, "FlaggedMessages.json");
            SaveMessages(new List<MailMessage>(), "DraftMessages.json"); //clear draft messages after syncing

            return messages;
        }

        private async Task<Dictionary<IMessageSummary, MimeMessage>> GetMimeMessagesAsync(SpecialFolder specialFolder, SearchQuery searchQuery, string subFolder = "") => await _getService().LoadMessages(specialFolder, searchQuery, subFolder);

        private List<MailMessage> ConvertFromMimeMessageToMailMessage(Dictionary<IMessageSummary, MimeMessage> messages)
        {
            var mailMessages = new List<MailMessage>();

            foreach (var mimeMessage in messages)
                mailMessages?.Add(CreateMessage(mimeMessage));

            return mailMessages;
        }

        private MailMessage CreateMessage(KeyValuePair<IMessageSummary, MimeMessage> mimeMessage)
        {
            var message = new MailMessage
            {
                Uid = mimeMessage.Key.UniqueId.Id,
                Subject = mimeMessage.Value.Subject,
                Body = mimeMessage.Value.HtmlBody ?? "",
                Attachments = ConvertMimeAttachmentsToMailMessageAttachments(mimeMessage.Value.Attachments),
                From = mimeMessage.Value.From[0].Name,
                Date = mimeMessage.Value.Date,
                IsSeen = mimeMessage.Key.Flags.Value.HasFlag(MessageFlags.Seen)
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

        private void SendNotificationAboutNewMessages(List<MailMessage> messages, List<MailMessage> originalMessages)
        {
            if (originalMessages != messages)
                new ToastContentBuilder()
                        .AddArgument("action", "viewConversation")
                        .AddText($"You have recieved new messages!")
                        .AddText($"Check this out.")
                        .Show();
        }

        private void SaveMessages(List<MailMessage> messages, string name) => JSONSaverAndReaderHelper.Save(messages, name);
        #endregion

        #region Flag message
        private async Task FlagMessage(MailMessage message)
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

            var allMessagesInFolder = await JSONSaverAndReaderHelper.Read<List<MailMessage>>(_messageFolder);
            var allMessages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("AllMessages.json");

            allMessages.FirstOrDefault(m => m.Uid == flagMessage.Uid).IsFlagged = flagMessage.IsFlagged;

            Messages = new ObservableCollection<MailMessage>(ReplaceMessageInCollection(message, flagMessage, allMessagesInFolder));

            await JSONSaverAndReaderHelper.Save(flaggedMessages, "FlaggedMessages.json");
            await JSONSaverAndReaderHelper.Save(allMessages, "AllMessages.json");

            if (_messageFolder != "FlaggedMessages.json")
                await JSONSaverAndReaderHelper.Save(allMessagesInFolder, _messageFolder);
        }
        #endregion

        #region Delete message
        private async Task DeleteMessage(MailMessage message)
        {
            Messages?.Remove(message);
            await JSONSaverAndReaderHelper.Save(Messages, _messageFolder);
        }
        #endregion

        #region Archive message
        private async Task ArchiveMessage(MailMessage message)
        {
            var archiveMessages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("ArchiveMessages.json");
            archiveMessages.Add(message);
            await JSONSaverAndReaderHelper.Save(archiveMessages, "ArchiveMessages.json");

            await DeleteMessage(message);
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

        private async Task UpdateMessages(MailMessage message, MailMessage messageForReplace)
        {
            Messages = new ObservableCollection<MailMessage>(ReplaceMessageInCollection(message, messageForReplace, await JSONSaverAndReaderHelper.Read<List<MailMessage>>(_messageFolder)));
            await JSONSaverAndReaderHelper.Save(Messages, _messageFolder);
        }

        private List<MailMessage> ReplaceMessageInCollection(MailMessage message, MailMessage messageForReplace, List<MailMessage> messages)
        {
            for (int i = 0; i < messages.Count; i++)
                if (messages[i].Equals(message))
                    messages[i] = messageForReplace;

            return messages;
        }
    }
}
