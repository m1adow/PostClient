using PostClient.ViewModels.Infrastructure;
using System.Windows.Input;
using System;
using Windows.UI.Xaml;
using PostClient.Models;
using PostClient.ViewModels.Helpers;
using System.Collections.ObjectModel;
using PostClient.Models.Services;
using System.Collections.Generic;
using MimeKit;
using Windows.UI.Popups;

namespace PostClient.ViewModels
{
    internal sealed class PostClientViewModel : ViewModelBase
    {
        public ObservableCollection<MailMessage> Messages { get; private set; } = new ObservableCollection<MailMessage>();

        private MailMessage _selectedMailMessage = new MailMessage();

        public MailMessage SelectedMailMessage
        {
            get => _selectedMailMessage;
            set
            {
                if (value == null)
                    value = new MailMessage();

                Set(ref _selectedMailMessage, value);

                if (_selectedMailMessage.Body.Length > 0)
                    MessageBodyControlsVisibility = Visibility.Visible;
            }
        }

        private Visibility _messageBodyControlsVisibility = Visibility.Collapsed;

        public Visibility MessageBodyControlsVisibility
        {
            get => _messageBodyControlsVisibility;
            set => Set(ref _messageBodyControlsVisibility, value);
        }

        private Visibility _managmentButtonsVisibility = Visibility.Visible;

        public Visibility ManagmentButtonsVisibility
        {
            get => _managmentButtonsVisibility;
            set => Set(ref _managmentButtonsVisibility, value);
        }

        private Visibility _loginControlsVisibility = Visibility.Collapsed;

        public Visibility LoginControlsVisibility
        {
            get => _loginControlsVisibility;
            set => Set(ref _loginControlsVisibility, value);
        }

        private Visibility _sendMessageControlsVisibility = Visibility.Collapsed;

        public Visibility SendMessageControlsVisibility
        {
            get => _sendMessageControlsVisibility;
            set => Set(ref _sendMessageControlsVisibility, value);
        }

        private bool _isRememberMeChecked = false;

        public bool IsRememberMeChecked
        {
            get => _isRememberMeChecked;
            set => Set(ref _isRememberMeChecked, value);
        }

        private string _email = string.Empty;

        public string Email
        {
            get => _email;
            set => Set(ref _email, value, new ICommand[] { LoginCommand });
        }

        private string _password = string.Empty;

        public string Password
        {
            get => _password;
            set => Set(ref _password, value, new ICommand[] { LoginCommand });
        }

        private bool _isGmailRadioButtonChecked = true;

        public bool IsGmailRadioButtonChecked
        {
            get => _isGmailRadioButtonChecked;
            set => Set(ref _isGmailRadioButtonChecked, value);
        }

        private bool _isOutlookRadioButtonChecked = false;

        public bool IsOutlookRadioButtonChecked
        {
            get => _isOutlookRadioButtonChecked;
            set => Set(ref _isOutlookRadioButtonChecked, value);
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
            set => Set(ref _messageReciever, value, new ICommand[] { SendMessageCommand });
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

        public ICommand SendMessageCommand { get; private set; }

        public ICommand CancelSendingMessageCommand { get; private set; }

        public ICommand LoginCommand { get; private set; }

        public ICommand SendCommand { get; private set; }

        public ICommand ShowLoginControlsCommand { get; private set; }

        public ICommand CancelLoginControlsCommand { get; private set; }

        public ICommand LoadCommand { get; private set; }

        public ICommand LoadNextListOfMessagesCommand { get; private set; }

        public ICommand LoadPreviousListOfMessagesCommand { get; private set; }

        public ICommand CloseMessageCommand { get; private set; }

        private List<MimeMessage> _mimeMessages = new List<MimeMessage>();

        private int[] _countOfMessages = new int[2] { 0, 10 };

        private Account _account = new Account();

        public PostClientViewModel()
        {
            SendMessageCommand = new RelayCommand(SendMessage, IsSendMessageFieldsFilled);
            CancelSendingMessageCommand = new RelayCommand(CancelSendingMessage);
            LoginCommand = new RelayCommand(LoginIntoAccount, IsLoginFieldsFilled);
            SendCommand = new RelayCommand(ShowSendMessageControlsAndLoadAccount);
            ShowLoginControlsCommand = new RelayCommand(ShowLoginControls);
            CancelLoginControlsCommand = new RelayCommand(HideLoginControls);
            LoadCommand = new RelayCommand(LoadMessages);
            LoadNextListOfMessagesCommand = new RelayCommand(LoadNextListOfMessages);
            LoadPreviousListOfMessagesCommand = new RelayCommand(LoadPreviousListOfMessages);
            CloseMessageCommand = new RelayCommand(CloseMessage);
        }

        #region Methods for sending message
        private void SendMessage()
        {  
            switch (_account.PostServiceName)
            {
                case nameof(GmailService):
                    new GmailService().SendMessage(_account, CreateMessage());
                    break;
                case nameof(OutlookService):
                    new OutlookService().SendMessage(_account, CreateMessage());
                    break;
            }

            ShowMessageDialog("Mail has sent successfully");
            SendMessageControlsVisibility = Visibility.Collapsed;
        }

        private MimeMessage CreateMessage()
        {
            MimeMessage message = new MimeMessage();

            message.From.Add(new MailboxAddress(MessageName, _account.Email));
            message.To.Add(MailboxAddress.Parse(MessageReciever));
            message.Subject = MessageSubject;
            message.Body = new TextPart()
            {
                Text = MessageBody
            };

            return message;
        }

        private bool IsSendMessageFieldsFilled() => MessageReciever.Length > 0;
        #endregion

        #region Methods for cancel sending 
        private void CancelSendingMessage() => SendMessageControlsVisibility = Visibility.Collapsed;
        #endregion

        #region Methods for login command
        private void LoginIntoAccount()
        {
            _account = new Account()
            {
                Email = this.Email,
                Password = this.Password,
                PostServiceName = GetServiceName()
            };

            if (IsRememberMeChecked)
                JSONSaverAndReaderHelper.Save(_account);

            ClearFields();
            HideLoginControls();
            LoadMessages();
        }

        private string GetServiceName()
        {
            if (IsGmailRadioButtonChecked)
                return nameof(GmailService);
            else
                return nameof(OutlookService);
        }

        private void ClearFields()
        {
            Email = string.Empty;
            Password = string.Empty;
        }

        private bool IsLoginFieldsFilled() => Email.Length > 0 && Password.Length > 0 && IsGmailRadioButtonChecked || IsOutlookRadioButtonChecked;
        #endregion

        #region Method for send command
        private async void ShowSendMessageControlsAndLoadAccount() 
        {
            if (_account.Email == null && _account.Password == null && _account.PostServiceName == null)
            {
                _account = await JSONSaverAndReaderHelper.Read();
                MessageSender = _account.Email;
            }

            SendMessageControlsVisibility = Visibility.Visible;
        } 
        #endregion

        #region Method for showing login controls command
        private void ShowLoginControls()
        {
            ManagmentButtonsVisibility = Visibility.Collapsed;
            LoginControlsVisibility = Visibility.Visible;
        }
        #endregion

        #region Method for showing login controls command
        private void HideLoginControls()
        {
            ManagmentButtonsVisibility = Visibility.Visible;
            LoginControlsVisibility = Visibility.Collapsed;
        }
        #endregion


        #region Method for load messages
        private async void LoadMessages()
        {
            if (_account.Email == null && _account.Password == null && _account.PostServiceName == null)
                _account = await JSONSaverAndReaderHelper.Read();

            switch (_account.PostServiceName)
            {
                case nameof(GmailService):
                    AddMessagesToCollection(new GmailService().LoadMessages(_account));
                    break;
                case nameof(OutlookService):
                    AddMessagesToCollection(new OutlookService().LoadMessages(_account));
                    break;
            }
        }

        private void AddMessagesToCollection(List<MimeMessage> messages)
        {
            Messages.Clear();

            if (_mimeMessages != messages)
                _mimeMessages = messages;

            int indexOfLastMessage = messages.Count - _countOfMessages[0];
            int indexOfFirstMessage = messages.Count < _countOfMessages[1] ? 0 : messages.Count - _countOfMessages[1];

            try
            {
                CheckForOutOfBounds(indexOfLastMessage, messages.Count, indexOfFirstMessage);

                for (int i = indexOfLastMessage - 1; i > indexOfFirstMessage - 1; i--)
                {
                    MailMessage mailMessage = CreateMessage(messages[i]);
                    Messages.Add(mailMessage);
                }
            }
            catch (Exception exception)
            {
                ShowMessageDialog(exception.Message);
            }            
        }

        private void CheckForOutOfBounds(int last, int max, int first)
        {
            if (last > max)
                throw new ArgumentOutOfRangeException("You've reached last list of messages");
            if (first < 0)
                throw new ArgumentOutOfRangeException("You've reached first list of messages");
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
        #endregion

        #region Method for load next list of messages
        private void LoadNextListOfMessages()
        {
            _countOfMessages = new int[] { _countOfMessages[0] + 10, _countOfMessages[1] + 10 };
            AddMessagesToCollection(_mimeMessages);
        }
        #endregion

        #region Method for load previous list of messages
        private void LoadPreviousListOfMessages()
        {
            _countOfMessages = new int[] { _countOfMessages[0] - 10, _countOfMessages[1] - 10 };
            AddMessagesToCollection(_mimeMessages);
        }
        #endregion

        #region Method for closing message
        private void CloseMessage()
        {
            _selectedMailMessage = new MailMessage();

            MessageBodyControlsVisibility = Visibility.Collapsed;
        }
        #endregion

        private async void ShowMessageDialog(string message)
        {
            MessageDialog messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        }
    }
}
