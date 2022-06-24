using PostClient.ViewModels.Infrastructure;
using System.Windows.Input;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PostClient.Views;
using PostClient.Models;
using PostClient.ViewModels.Helpers;
using System.Collections.ObjectModel;
using PostClient.Models.Services;

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

                if (_selectedMailMessage != null && _selectedMailMessage.Body != null)
                {
                    RightSideControlsVisibility = Visibility.Collapsed;
                    MessageBodyControlsVisibility = Visibility.Visible;
                }
            }
        }

        private Visibility _rightSideControlsVisibility = Visibility.Visible;

        public Visibility RightSideControlsVisibility
        {
            get => _rightSideControlsVisibility;
            set => Set(ref _rightSideControlsVisibility, value);
        }

        private Visibility _messageBodyControlsVisibility = Visibility.Collapsed;

        public Visibility MessageBodyControlsVisibility
        {
            get => _messageBodyControlsVisibility;
            set => Set(ref _messageBodyControlsVisibility, value);
        }

        public ICommand SendCommand { get; private set; }

        public ICommand LoginCommand { get; private set; }

        public ICommand LoadCommand { get; private set; }

        public ICommand LoadNextListOfMessagesCommand { get; private set; }

        public ICommand LoadPreviousListOfMessagesCommand { get; private set; }

        public ICommand CloseMessageCommand { get; private set; }

        private int[] _countOfMessages = new int[2] { 0, 5 };

        public PostClientViewModel()
        {
            SendCommand = new RelayCommand(OpenSendMessagePage);
            LoginCommand = new RelayCommand(OpenLoginPage);
            LoadCommand = new RelayCommand(LoadMessages);
            LoadNextListOfMessagesCommand = new RelayCommand(LoadNextListOfMessages);
            LoadPreviousListOfMessagesCommand = new RelayCommand(LoadPreviousListOfMessages);
            CloseMessageCommand = new RelayCommand(CloseMessage);
        }

        #region Method for send command
        private void OpenSendMessagePage() => ShowPage(nameof(SendMessagePage));
        #endregion

        #region Method for login command
        private void OpenLoginPage() => ShowPage(nameof(LoginPage));
        #endregion

        private async void ShowPage(string page)
        {
            var view = CoreApplication.CreateNewView();

            int viewId = 0;

            await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    Frame frame = new Frame();

                    if (page == nameof(SendMessagePage))
                        frame.Navigate(typeof(SendMessagePage));
                    else if (page == nameof(LoginPage))
                        frame.Navigate(typeof(LoginPage));

                    Window.Current.Content = frame;
                    Window.Current.Activate();

                    viewId = ApplicationView.GetForCurrentView().Id;
                });

            await ApplicationViewSwitcher.TryShowAsStandaloneAsync(viewId, ViewSizePreference.UseMinimum);
        }

        #region Method for load messages
        private void LoadMessages() => LoadMessagesWithCount(_countOfMessages);
        #endregion

        #region Method for load next list of messages
        private void LoadNextListOfMessages()
        {
            _countOfMessages = new int[] { _countOfMessages[0] + 5, _countOfMessages[1] + 5 };
            LoadMessagesWithCount(_countOfMessages);
        }
        #endregion

        #region Method for load previous list of messages
        private void LoadPreviousListOfMessages()
        {
            _countOfMessages = new int[] { _countOfMessages[0] - 5, _countOfMessages[1] - 5 };
            LoadMessagesWithCount(_countOfMessages);
        }
        #endregion

        private async void LoadMessagesWithCount(int[] count)
        {
            Account account = await JSONSaverAndReaderHelper.Read();

            switch (account.PostServiceName)
            {
                case nameof(GmailService):
                    AddMessagesToCollection(new GmailService().LoadMessages(account, count));
                    break;
                case nameof(OutlookService):
                    AddMessagesToCollection(new OutlookService().LoadMessages(account, count));
                    break;
            }
        }

        private void AddMessagesToCollection(ObservableCollection<MailMessage> messages)
        {
            Messages.Clear();

            foreach (var message in messages)
                Messages.Add(message);
        }

        #region Method for closing message
        private void CloseMessage()
        {
            _selectedMailMessage = new MailMessage();

            RightSideControlsVisibility = Visibility.Visible;
            MessageBodyControlsVisibility = Visibility.Collapsed;
        }
        #endregion
    }
}
