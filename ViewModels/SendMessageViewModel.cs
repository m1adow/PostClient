using MimeKit;
using PostClient.Models;
using PostClient.Models.Services;
using PostClient.ViewModels.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace PostClient.ViewModels
{
    internal sealed class SendMessageViewModel : ViewModelBase
    {
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
            set => Set(ref _messageReciever, value, new ICommand[] { SendMessageCommand, DraftMessageCommand });
        }

        private string _messageName = "Username";

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

        private string _selectedText = string.Empty;

        public string SelectedText
        {
            get => _selectedText;
            set => Set(ref _selectedText, value);
        }

        private Visibility _sendMessageControlsVisibility = Visibility.Collapsed;

        public Visibility SendMessageControlsVisibility
        {
            get => _sendMessageControlsVisibility;
            set => Set(ref _sendMessageControlsVisibility, value);
        }

        public ICommand SendMessageCommand { get; }

        public ICommand InsertFileCommand { get; }

        public ICommand DraftMessageCommand { get; }

        public ICommand CancelSendingMessageCommand { get; }

        public ICommand ShowSendingControlsCommand { get; }

        public ICommand HideSendingControlsCommand { get; }

        public ICommand BoldSelectedTextCommand { get; }

        public ICommand ItalicSelectedTextCommand { get; }

        public ICommand UnderlineSelectedTextCommand { get; }

        public ICommand AddLineCommand { get; }

        public Func<Visibility, MailMessage, bool> ChangeSendMessageControlsVisibilityAndFillFieldsFunc { get; }

        private Account _account = new Account();

        private MailMessage _selectedMessage = new MailMessage();

        private readonly Func<Account> _getAccount;

        private readonly Func<MailMessage, bool> _deleteDraft;

        private Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>();

        public SendMessageViewModel(Func<Account> getAccount, Func<MailMessage, bool> deleteDraft)
        {
            _getAccount = getAccount;
            _account = getAccount();
            _deleteDraft = deleteDraft;

            ChangeSendMessageControlsVisibilityAndFillFieldsFunc = ChangeSendMessageControlsVisibilityAndFillFields;

            SendMessageCommand = new RelayCommand(SendMessage, IsSendMessageFieldsFilled);
            InsertFileCommand = new RelayCommand(InsertFile);
            DraftMessageCommand = new RelayCommand(DraftMessage, IsSendMessageFieldsFilled);
            CancelSendingMessageCommand = new RelayCommand(CancelSendingMessage);
            ShowSendingControlsCommand = new RelayCommand(ShowSendMessageControlsAndLoadAccount);
            HideSendingControlsCommand = new RelayCommand(HideSendMessageControls);
            BoldSelectedTextCommand = new RelayCommand(BoldSelectedText);
            ItalicSelectedTextCommand = new RelayCommand(ItalicSelectedText);
            UnderlineSelectedTextCommand = new RelayCommand(UnderlineSelectedText);
            AddLineCommand = new RelayCommand(AddLine);
        }

        #region Methods for sending message
        private void SendMessage()
        {
            MimeMessage message = CreateMessage();

            switch (_account.PostServiceName)
            {
                case nameof(GmailService):
                    new GmailService(_account).SendMessage(message, MessageDialogShower.ShowMessageDialog);
                    break;
                case nameof(OutlookService):
                    new OutlookService(_account).SendMessage(message, MessageDialogShower.ShowMessageDialog);
                    break;
            }

            if (_selectedMessage.IsDraft)
                _deleteDraft(_selectedMessage);

            MessageDialogShower.ShowMessageDialog("Mail has sent successfully");
            ClearFields();
            SendMessageControlsVisibility = Visibility.Collapsed;
            _files.Clear();
        }

        private MimeMessage CreateMessage()
        {
            MimeMessage message = new MimeMessage();

            message.From.Add(new MailboxAddress(MessageName, _account.Email));
            message.To.Add(MailboxAddress.Parse(MessageReciever));
            message.Subject = MessageSubject;

            BodyBuilder builder = new BodyBuilder();

            builder.HtmlBody = MessageBody;

            if (_files.Count > 0)
                foreach (var file in _files)
                    builder.Attachments.Add(file.Key, file.Value);

            message.Body = builder.ToMessageBody();

            return message;
        }

        private void ClearFields()
        {
            MessageReciever = string.Empty;
            MessageName = "New message";
            MessageSubject = "It's my beautiful post app";
            MessageBody = "Hi world!";
        }

        private bool IsSendMessageFieldsFilled() => MessageReciever.Length > 0;
        #endregion

        #region Methods for inserting files

        private async void InsertFile()
        {
            var file = await GetFileBytesAsync();
            _files.Add(file.Key, file.Value);
        }

        private async Task<KeyValuePair<string, byte[]>> GetFileBytesAsync()
        {
            KeyValuePair<string, byte[]> bytes = new KeyValuePair<string, byte[]>();

            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".txt");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                IBuffer buffer = await FileIO.ReadBufferAsync(file);
                bytes = new KeyValuePair<string, byte[]>(file.Name, WindowsRuntimeBufferExtensions.ToArray(buffer));
            }

            return bytes;
        }
        #endregion

        #region Method for draft message
        private async void DraftMessage()
        {
            List<MailMessage> draftMessages = await JSONSaverAndReaderHelper.Read<List<MailMessage>>("DraftMessages.json");

            draftMessages.Add(new MailMessage
            {
                Name = MessageName,
                Subject = MessageSubject,
                Body = MessageBody,
                Attachments = _files,
                From = _account.Email,
                To = MessageReciever,
                IsDraft = true
            });

            JSONSaverAndReaderHelper.Save(draftMessages, "DraftMessages.json");
            ClearFields();
        }
        #endregion

        #region Method for cancel sending 
        private void CancelSendingMessage()
        {
            SendMessageControlsVisibility = Visibility.Collapsed;
            ClearFields();
        }
        #endregion

        #region Method for showing and hiding send message controls command
        private void ShowSendMessageControlsAndLoadAccount()
        {
            SendMessageControlsVisibility = Visibility.Visible;

            _account = _getAccount();
            MessageSender = _account.Email;
        }

        private void HideSendMessageControls() => SendMessageControlsVisibility = Visibility.Collapsed;
        #endregion

        #region Method for change send controls visibility
        private bool ChangeSendMessageControlsVisibilityAndFillFields(Visibility visibility, MailMessage message)
        {
            SendMessageControlsVisibility = visibility;

            if (message.IsDraft)
            {
                MessageName = message.Name;
                MessageSubject = message.Subject;
                MessageBody = message.Body;
                MessageReciever = message.To;
            }

            _selectedMessage = message;

            return true;
        }
        #endregion

        #region Methods for styling text
        private void BoldSelectedText() => AcceptStyling("b");

        private void ItalicSelectedText() => AcceptStyling("i");

        private void UnderlineSelectedText() => AcceptStyling("u");

        private void AcceptStyling(string tag)
        {
            if (SelectedText.Contains($"<{tag}>") || SelectedText.Contains($"</{tag}>"))
            {
                SelectedText = SelectedText.Replace($"<{tag}>", "");
                SelectedText = SelectedText.Replace($"</{tag}>", "");
            }
            else
                SelectedText = $"<{tag}>{SelectedText}</{tag}>";
        }
        #endregion

        #region Method for adding line
        private void AddLine()
        {
            string tag = "<br>";

            if (SelectedText.Contains(tag))
                SelectedText = SelectedText.Replace($"\n{tag}\n", "");
            else
                SelectedText += $"\n{tag}\n";           
        }
        #endregion
    }
}
