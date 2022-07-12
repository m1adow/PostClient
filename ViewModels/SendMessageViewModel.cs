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
using Windows.ApplicationModel.Contacts;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        public ICommand StyleSelectedTextCommand { get; }

        public ICommand AddLineCommand { get; }

        public ICommand ChooseContactCommand { get; }

        public Func<Visibility, MailMessage, bool> ChangeSendMessageControlsVisibilityAndFillFieldsFunc { get; }

        private Account _account = new Account();

        private MailMessage _selectedMessage = new MailMessage();

        private readonly Func<Account> _getAccount;

        private readonly Func<MailMessage, bool> _deleteDraft;

        private List<KeyValuePair<string, byte[]>> _files = new List<KeyValuePair<string, byte[]>>();

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
            StyleSelectedTextCommand = new RelayCommand(StyleSelectedText);
            AddLineCommand = new RelayCommand(AddLine);
            ChooseContactCommand = new RelayCommand(ChooseContact);
        }

        #region Sending message
        private void SendMessage(object parameter)
        {
            MimeMessage message = CreateMessage();

            switch (_account.PostServiceName)
            {
                case nameof(GmailService):
                    new GmailService(_account).SendMessage(message);
                    break;
                case nameof(OutlookService):
                    new OutlookService(_account).SendMessage(message);
                    break;
            }

            if (_selectedMessage.IsDraft)
                _deleteDraft(_selectedMessage);

            MessageDialogShower.ShowMessageDialog("Mail has sent successfully");
            ClearFields(parameter as ComboBox);
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

        private void ClearFields(ComboBox comboBox)
        {
            MessageReciever = string.Empty;
            MessageName = "New message";
            MessageSubject = "It's my beautiful post app";
            MessageBody = "Hi world!";
            comboBox.Items.Clear();
        }

        private bool IsSendMessageFieldsFilled(object parameter) => MessageReciever.Length > 0;
        #endregion

        #region Inserting files

        private async void InsertFile(object parameter)
        {
            var file = await GetFileBytesAsync();
            _files.Add(file);

            ComboBox filesComboBox = parameter as ComboBox;
            filesComboBox.Items.Add(file.Key);
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

        #region Draft message
        private async void DraftMessage(object parameter)
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
            ClearFields(parameter as ComboBox);
        }
        #endregion

        #region Cancel sending 
        private void CancelSendingMessage(object parameter)
        {
            SendMessageControlsVisibility = Visibility.Collapsed;
            ClearFields(parameter as ComboBox);
        }
        #endregion

        #region Showing and hiding send message controls
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

        #region Styling text
        private void StyleSelectedText(object parameter)
        {
            string tag = parameter.ToString();

            if (SelectedText.Contains($"<{tag}>") || SelectedText.Contains($"</{tag}>"))
            {
                SelectedText = SelectedText.Replace($"<{tag}>", "");
                SelectedText = SelectedText.Replace($"</{tag}>", "");
            }
            else
                SelectedText = $"<{tag}>{SelectedText}</{tag}>";
        }
        #endregion

        #region Adding line
        private void AddLine(object parameter)
        {
            string tag = "<br>";

            if (SelectedText.Contains(tag))
                SelectedText = SelectedText.Replace($"\n{tag}\n", "");
            else
                SelectedText += $"\n{tag}\n";           
        }
        #endregion

        #region Choosing contact
        private async void ChooseContact(object parameter)
        {
            var contactPicker = new ContactPicker();
            var contact = await contactPicker.PickContactAsync();

            if (contact != null)
                MessageReciever = contact.Emails[0].Address;
        }
        #endregion
    }
}
