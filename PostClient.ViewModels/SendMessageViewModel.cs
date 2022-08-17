using MimeKit;
using PostClient.Models;
using PostClient.Models.Infrastructure;
using PostClient.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Contacts;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using RtfPipe;
using System.Timers;

namespace PostClient.ViewModels
{
#nullable enable

    public sealed class SendMessageViewModel : ViewModelBase
    {
        private string? _messageSender = string.Empty;

        public string? MessageSender
        {
            get => _messageSender;
            set => Set(ref _messageSender, value);
        }

        private string? _messageReciever = string.Empty;

        public string? MessageReciever
        {
            get => _messageReciever;
            set => Set(ref _messageReciever, value, new ICommand[] { SendMessageCommand, SendMessageWithDelayCommand, DraftMessageCommand });
        }

        private string? _messageName = string.Empty;

        public string? MessageName
        {
            get => _messageName;
            set => Set(ref _messageName, value);
        }

        private string? _messageSubject = string.Empty;

        public string? MessageSubject
        {
            get => _messageSubject;
            set => Set(ref _messageSubject, value);
        }

        private string? _messageBody = "";

        public string? MessageBody
        {
            get => _messageBody;
            set => Set(ref _messageBody, value);
        }

        private List<KeyValuePair<string, byte[]>>? _files = new List<KeyValuePair<string, byte[]>>();

        public List<KeyValuePair<string, byte[]>>? Files
        {
            get => _files;
            private set => Set(ref _files, value);
        }

        private Visibility? _sendMessageControlsVisibility = Visibility.Collapsed;

        public Visibility? SendMessageControlsVisibility
        {
            get => _sendMessageControlsVisibility;
            set => Set(ref _sendMessageControlsVisibility, value);
        }

        private TimeSpan _delayTime = new TimeSpan();

        public TimeSpan DelayTime
        {
            get => _delayTime;
            set => Set(ref _delayTime, value);
        }

        public ICommand SendMessageCommand { get; }

        public ICommand SendMessageWithDelayCommand { get; }

        public ICommand InsertFileCommand { get; }

        public ICommand DraftMessageCommand { get; }

        public ICommand CancelSendingMessageCommand { get; }

        public ICommand ShowSendingControlsCommand { get; }

        public ICommand HideSendingControlsCommand { get; }

        public ICommand ChooseContactCommand { get; }

        public Action<string> PasteTextAction { get; }

        public Func<Visibility, MailMessage, bool> ChangeSendMessageControlsVisibilityAndFillFieldsFunc { get; }

        private Account? _account;

        private MailMessage? _selectedMessage = new MailMessage();

        private readonly Func<Account> _getAccount;

        private readonly Func<IPostService> _getService;

        private readonly Func<MailMessage, Task> _deleteDraft;

        private MimeMessage _scheduledMessage = new MimeMessage();

        public SendMessageViewModel(Func<IPostService> getService, Func<Account> getAccount, Func<MailMessage, Task> deleteDraft)
        {                  
            _getAccount = getAccount;
            _account = getAccount();
            _getService = getService;
            _deleteDraft = deleteDraft;

            PasteTextAction = PasteText;
            ChangeSendMessageControlsVisibilityAndFillFieldsFunc = ChangeSendMessageControlsVisibilityAndFillFields;

            SendMessageCommand = new RelayCommand(SendMessage, IsSendMessageFieldsFilled);
            SendMessageWithDelayCommand = new RelayCommand(SendMessageWithDelay, IsSendMessageFieldsFilled);
            InsertFileCommand = new RelayCommand(InsertFile);
            DraftMessageCommand = new RelayCommand(DraftMessage, IsSendMessageFieldsFilled);
            CancelSendingMessageCommand = new RelayCommand(CancelSendingMessage);
            ShowSendingControlsCommand = new RelayCommand(ShowSendMessageControlsAndLoadAccount, CanSendMessage);
            HideSendingControlsCommand = new RelayCommand(HideSendMessageControls);
            ChooseContactCommand = new RelayCommand(ChooseContact);
        }

        #region Sending message
        private async void SendMessage(object parameter)
        {
            MimeMessage message = CreateMessage();

            await _getService().SendMessageAsync(message);

            if (_selectedMessage.IsDraft)
                await _deleteDraft(_selectedMessage);

            ContentDialogShower.ShowContentDialog("Notification", "Mail has sent successfully");

            var comboBox = parameter as ComboBox;

            if (comboBox != null)
                ClearFields(comboBox);

            SendMessageControlsVisibility = Visibility.Collapsed;
        }

        private MimeMessage CreateMessage()
        {
            MimeMessage message = new MimeMessage();

            message.From.Add(new MailboxAddress(MessageName, MessageSender));
            message.To.Add(MailboxAddress.Parse(MessageReciever));
            message.Subject = MessageSubject;

            var builder = new BodyBuilder();

            builder.HtmlBody = Rtf.ToHtml(MessageBody);

            if (Files?.Count > 0)
                foreach (var file in Files)
                    builder.Attachments.Add(file.Key, file.Value);

            message.Body = builder.ToMessageBody();

            return message;
        }

        private async void ClearFields(object parameter)
        {
            if (_selectedMessage.IsDraft)
                await _deleteDraft(_selectedMessage);

            var comboBox = parameter as ComboBox;
            comboBox?.Items.Clear();

            MessageReciever = string.Empty;
            MessageName = string.Empty;
            MessageSubject = string.Empty;
            MessageBody = "";
            Files?.Clear();
        }

        private bool IsSendMessageFieldsFilled(object parameter) => MessageReciever.Length > 0;
        #endregion

        #region Sending message with delay
        private void SendMessageWithDelay(object parameter)
        {
            _scheduledMessage = CreateMessage();
            ClearFields(parameter);

            DateTime dateTimeNow = DateTime.Now;
            DateTime scheduledTime = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour + DelayTime.Hours, dateTimeNow.Minute + DelayTime.Minutes, dateTimeNow.Second);

            if (dateTimeNow > scheduledTime)
                scheduledTime = scheduledTime.AddDays(1);

            double tickTime = (double)(scheduledTime - dateTimeNow).TotalMilliseconds;
            Timer timer = new Timer(tickTime);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e) => await _getService().SendMessageAsync(_scheduledMessage);
        #endregion

        #region Inserting files
        private async void InsertFile(object parameter)
        {
            var files = await GetFileBytesAsync();
            Files = Files.Union(files).ToList();

            ComboBox? filesComboBox = (parameter as ComboBox) ?? new ComboBox();
            filesComboBox.Items.Clear();
            Files.ForEach(f => filesComboBox.Items.Add(f.Key));
        }

        private async Task<List<KeyValuePair<string, byte[]>>> GetFileBytesAsync()
        {
            var bytes = new List<KeyValuePair<string, byte[]>>();

            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            openPicker.FileTypeFilter.Add(".txt");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".doxc");
            openPicker.FileTypeFilter.Add(".xlsx");
            openPicker.FileTypeFilter.Add(".rtf");
            openPicker.FileTypeFilter.Add(".pdf");

            var files = await openPicker.PickMultipleFilesAsync();

            if (files != null)
            {
                foreach (var file in files)
                {
                    IBuffer buffer = await FileIO.ReadBufferAsync(file);
                    bytes.Add(new KeyValuePair<string, byte[]>(file.Name, WindowsRuntimeBufferExtensions.ToArray(buffer)));
                }
            }

            return bytes;
        }
        #endregion

        #region Draft message
        private async void DraftMessage(object parameter)
        {
            var draftMessages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("DraftMessages.json");

            draftMessages.Add(new MailMessage
            {
                Name = MessageName,
                Subject = MessageSubject,
                Body = MessageBody,
                Attachments = Files,
                From = _account?.Email,
                To = MessageReciever,
                Uid = 1,
                IsDraft = true
            });

            await JSONSaverAndReaderHelper.Save(draftMessages, "DraftMessages.json");
            ClearFields((parameter as ComboBox) ?? new ComboBox());
        }
        #endregion

        #region Cancel sending 
        private void CancelSendingMessage(object parameter)
        {
            SendMessageControlsVisibility = Visibility.Collapsed;
            ClearFields((parameter as ComboBox) ?? new ComboBox());
        }
        #endregion

        #region Showing and hiding send message controls
        private bool CanSendMessage(object parameter) => _getService() != null;

        private void ShowSendMessageControlsAndLoadAccount(object parameter)
        {
            SendMessageControlsVisibility = Visibility.Visible;

            _account = _getAccount();
            MessageSender = _account.Email;
        }

        private void HideSendMessageControls(object parameter)
        {
            _account = _getAccount();

            if (!_selectedMessage.IsDraft)
                SendMessageControlsVisibility = Visibility.Collapsed;
        }
        #endregion

        #region Changing send controls visibility
        private bool ChangeSendMessageControlsVisibilityAndFillFields(Visibility visibility, MailMessage message)
        {
            SendMessageControlsVisibility = visibility;

            MessageName = message.Name;
            MessageSubject = message.Subject;
            MessageBody = message.Body;
            MessageReciever = message.To;
            Files = message.Attachments;

            _selectedMessage = message;

            return true;
        }
        #endregion

        #region Choosing contact
        private async void ChooseContact(object parameter)
        {
            var contactPicker = new ContactPicker();
            var contact = await contactPicker.PickContactAsync();

            if (contact != null)
            {
                MenuFlyout menuFlyout = new MenuFlyout();

                foreach (var email in contact.Emails)
                {
                    var menuFlyoutItem = new MenuFlyoutItem() { Text = email.Address };
                    menuFlyoutItem.Click += MenuFlyoutItem_Click;
                    menuFlyout.Items.Add(menuFlyoutItem);
                }

                menuFlyout.ShowAt(parameter as AppBarButton);            
            }           
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e) => MessageReciever = (sender as MenuFlyoutItem)?.Text;
        #endregion

        private void PasteText(string text) => MessageBody = text;
    }
}
