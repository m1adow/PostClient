using MimeKit;
using PostClient.Models;
using PostClient.Models.Services;
using PostClient.ViewModels.Helpers;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private Visibility _sendMessageControlsVisibility = Visibility.Collapsed;

        public Visibility SendMessageControlsVisibility
        {
            get => _sendMessageControlsVisibility;
            set => Set(ref _sendMessageControlsVisibility, value);
        }

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

        private string _messageName = "New message";

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

        public ICommand SendMessageCommand { get; }

        public ICommand InsertFileCommand { get; }

        public ICommand DraftMessageCommand { get; }

        public ICommand CancelSendingMessageCommand { get; }

        public ICommand ShowSendingControlosCommand { get; }

        public Func<Visibility, MailMessage, bool> ChangeSendMessageControlsVisibilityAndFillFieldsFunc { get; }

        private Account _account = new Account();

        private Func<Account> _getAccount;

        private MailMessage _selectedMessage = new MailMessage();

        private Func<MailMessage, bool> _deleteDraft;

        private List<byte[]> _files = new List<byte[]>();

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
            ShowSendingControlosCommand = new RelayCommand(ShowSendMessageControlsAndLoadAccount);
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

            builder.TextBody = MessageBody;
            
            if (_files.Count > 0)
                foreach (var file in _files)
                    builder.Attachments.Add("attachment.txt", file);

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
            byte[] bytes = await GetFileBytesAsync();
            _files.Add(bytes);
        }

        private async Task<byte[]> GetFileBytesAsync()
        {
            List<byte> bytes = new List<byte>();

            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".txt");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                IBuffer buffer = await FileIO.ReadBufferAsync(file);
                bytes = WindowsRuntimeBufferExtensions.ToArray(buffer).ToList();
            }
            else
            {

            }

            return bytes.ToArray();
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

        #region Method for send command
        private void ShowSendMessageControlsAndLoadAccount()
        {
            SendMessageControlsVisibility = Visibility.Visible;

            _account = _getAccount();
            MessageSender = _account.Email;
        }
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
    }
}
