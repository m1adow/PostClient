using MailKit;
using PostClient.Models;
using PostClient.Models.Infrastructure;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PostClient.ViewModels
{
#nullable enable

    public sealed class ControlMessageViewModel : ViewModelBase
    {
        private MailMessage? _selectedMailMessage = new MailMessage();

        public MailMessage? SelectedMailMessage
        {
            get => _selectedMailMessage;
            set
            {
                if (value == null)
                    value = new MailMessage();

                Set(ref _selectedMailMessage, value);
            }
        }

        private Visibility? _messageViewConrtolVisibility = Visibility.Collapsed;

        public Visibility? MessageViewConrtolVisibility
        {
            get => _messageViewConrtolVisibility;
            set => Set(ref _messageViewConrtolVisibility, value);
        }

        private Visibility? _searchBoxControlVisibility = Visibility.Visible;

        public Visibility? SearchBoxControlsVisibility
        {
            get => _searchBoxControlVisibility;
            set => Set(ref _searchBoxControlVisibility, value);
        }

        private Visibility? _actsControlsVisibility = Visibility.Collapsed;

        public Visibility? ActsControlsVisibility
        {
            get => _actsControlsVisibility;
            set => Set(ref _actsControlsVisibility, value);
        }

        private ListViewSelectionMode? _listViewWithMessagesSelectionMode = ListViewSelectionMode.Single;

        public ListViewSelectionMode? ListViewWithMessagesSelectionMode
        {
            get => _listViewWithMessagesSelectionMode;
            set => Set(ref _listViewWithMessagesSelectionMode, value);
        }

        public ICommand FlagMessageCommand { get; }

        public ICommand DeleteMessageCommand { get; }

        public ICommand ArchiveMessageCommand { get; }

        public ICommand UnseenMessageCommand { get; }

        public ICommand CloseMessageCommand { get; }

        public ICommand HideMessageViewCommand { get; }

        public ICommand ChangeMessageOnRightTapCommand { get; }

        public ICommand ReplyMessageCommand { get; }

        public ICommand ChangeListViewSelectionCommand { get; }

        public ICommand SelectionChangedHandlingCommand { get; }

        public Action<Visibility> ChangeSearchBoxControlVisibilityAction { get; }

        private readonly Func<MailMessage, Task> _updateFlaggedList;

        private readonly Func<MailMessage, Task> _deleteMessageFromList;

        private readonly Func<Visibility, MailMessage, bool> _changeSendMessageControlsVisibilityAndMessage;

        private readonly Func<IPostService> _getService;

        private readonly Func<MailMessage, Task> _archiveMessageAction;

        private readonly Func<MailMessage, MailMessage, Task> _updateMessagesAction;

        private List<MailMessage>? _selectedMailMessages = new List<MailMessage>();

        public ControlMessageViewModel(Func<IPostService> getService, Func<MailMessage, Task> updateFlaggedList, Func<MailMessage, Task> deleteMessageFromList, Func<Visibility, MailMessage, bool> changeSendMessageControlsVisibilityAndMessage, Func<MailMessage, Task> archiveMessageAction, Func<MailMessage, MailMessage, Task> updateMessagesAction)
        {
            _getService = getService;
            _updateFlaggedList = updateFlaggedList;
            _deleteMessageFromList = deleteMessageFromList;
            _changeSendMessageControlsVisibilityAndMessage = changeSendMessageControlsVisibilityAndMessage;
            _archiveMessageAction = archiveMessageAction;
            _updateMessagesAction = updateMessagesAction;

            FlagMessageCommand = new RelayCommand(FlagMessage);
            DeleteMessageCommand = new RelayCommand(DeleteMessage);
            CloseMessageCommand = new RelayCommand(CloseMessage);
            ArchiveMessageCommand = new RelayCommand(ArchiveMessage);
            UnseenMessageCommand = new RelayCommand(UnseenMessage);
            HideMessageViewCommand = new RelayCommand(HideMessageView);
            ChangeMessageOnRightTapCommand = new RelayCommand(ChangeMessageOnRightTap);
            ReplyMessageCommand = new RelayCommand(ReplyMessage);
            ChangeListViewSelectionCommand = new RelayCommand(ChangeListViewSelection);
            SelectionChangedHandlingCommand = new RelayCommand(SelectionChangedHandling);

            ChangeSearchBoxControlVisibilityAction = ChangeSearchBoxControlVisibility;
        }

        #region Flag message
        private async void FlagMessage(object parameter)
        {
            if (_selectedMailMessages.Count > 0)
            {
                foreach (var message in _selectedMailMessages)
                    await FlagSpecificMessageAsync(message);
            }
            else
                await FlagSpecificMessageAsync(SelectedMailMessage);
        }

        private async Task FlagSpecificMessageAsync(MailMessage message)
        {
            if (message.Uid != 0)
            {
                if (!message.IsPopMessage)
                    await _getService().FlagMessageAsync(message, MessageFlags.Flagged, GetSpecialFolder(message.Folder), message.Folder);
                await _updateFlaggedList(message);
            }
        }
        #endregion

        #region Delete message
        private async void DeleteMessage(object parameter)
        {
            if (_selectedMailMessages.Count > 0)
            {
                foreach (var message in _selectedMailMessages)
                    await DeleteSpecificMessageAsync(message);
            }
            else
                await DeleteSpecificMessageAsync(SelectedMailMessage);

            CloseMessage(parameter);
        }

        private async Task DeleteSpecificMessageAsync(MailMessage message)
        {
            if (message.Uid != 0)
            {
                if (!message.IsDraft && !message.IsPopMessage)
                    await _getService().FlagMessageAsync(message, MessageFlags.Deleted, GetSpecialFolder(message.Folder), message.Folder);
                else if (message.IsPopMessage)
                    await _getService().DeletePopMessageAsync(Convert.ToInt32(message.Uid));
                await _deleteMessageFromList(message);
            }
        }
        #endregion

        #region Archive message
        private async void ArchiveMessage(object parameter)
        {
            if (_selectedMailMessages.Count > 0)
            {
                foreach (var message in _selectedMailMessages)
                    await ArchiveSpecificMessageAsync(message);
            }
            else
                await ArchiveSpecificMessageAsync(SelectedMailMessage);
        }

        private async Task ArchiveSpecificMessageAsync(MailMessage message)
        {
            if (message.Uid != 0)
                await _archiveMessageAction(message);
        }
        #endregion

        #region Unseen message
        private async void UnseenMessage(object parameter)
        {
            if (_selectedMailMessages.Count > 0)
            {
                foreach (var message in _selectedMailMessages)
                   await UnseenSpecificMessageAsync(message);
            }
            else
                await UnseenSpecificMessageAsync(SelectedMailMessage);
        }

        private async Task UnseenSpecificMessageAsync(MailMessage message)
        {
            if (message.IsSeen && message.Uid != 0 && !message.IsPopMessage)
            {
                await _getService().FlagMessageAsync(message, MessageFlags.Seen, GetSpecialFolder(message.Folder), message.Folder);
                message.IsSeen = false;
                await _updateMessagesAction(new MailMessage { Uid = message.Uid }, message);
            }
        }
        #endregion

        #region Closing message
        private void CloseMessage(object parameter)
        {
            _selectedMailMessage = new MailMessage();

            MessageViewConrtolVisibility = Visibility.Collapsed;
            _changeSendMessageControlsVisibilityAndMessage(Visibility.Collapsed, _selectedMailMessage);
        }
        #endregion

        #region Hiding message view
        private void HideMessageView(object parameter) => MessageViewConrtolVisibility = Visibility.Collapsed;
        #endregion

        #region Change message on right tap
        private void ChangeMessageOnRightTap(object parameter) => SelectedMailMessage = parameter as MailMessage;
        #endregion

        #region Reply message
        private void ReplyMessage(object parameter)
        {
            MessageViewConrtolVisibility = Visibility.Collapsed;
            _changeSendMessageControlsVisibilityAndMessage(Visibility.Visible,
                new MailMessage()
                {
                    Subject = $"RE: {SelectedMailMessage.Subject}",
                    To = SelectedMailMessage.From,
                    Body = $"\n\n\n\n\n\n\n\n\nFrom: {SelectedMailMessage.From}\nSent: {SelectedMailMessage.Date}\nTo: {SelectedMailMessage.To}\nSubject: {SelectedMailMessage.Subject}"
                });
        }
        #endregion

        #region Change selection
        private void ChangeListViewSelection(object parameter)
        {
            if (ListViewWithMessagesSelectionMode == ListViewSelectionMode.Single)
            {
                ListViewWithMessagesSelectionMode = ListViewSelectionMode.Multiple;
                ActsControlsVisibility = Visibility.Visible;
                SearchBoxControlsVisibility = Visibility.Collapsed;
            }

            else
            {
                ListViewWithMessagesSelectionMode = ListViewSelectionMode.Single;
                ActsControlsVisibility = Visibility.Collapsed;
                SearchBoxControlsVisibility = Visibility.Visible;
            }
        }
        #endregion

        #region Selection changed handling
        private void SelectionChangedHandling(object parameter)
        {
            var listView = parameter as ListView;

            if (ListViewWithMessagesSelectionMode == ListViewSelectionMode.Single)
            {
                var message = listView?.SelectedItem as MailMessage;

                if (message != null)
                    ChangeSelectedMessage(message);
            }
            else
            {
                var messages = new List<MailMessage>();

                foreach (var item in listView?.SelectedItems)
                {
                    if (item is MailMessage message)
                        messages.Add(message);
                }

                if (messages != null)
                    _selectedMailMessages = messages.ToList();
            }
        }

        private async void ChangeSelectedMessage(MailMessage message)
        {
            _selectedMailMessages = new List<MailMessage>();

            SelectedMailMessage = message;

            if (SelectedMailMessage.Body.Length > 0 && !SelectedMailMessage.IsDraft)
                MessageViewConrtolVisibility = Visibility.Visible;
            else if (SelectedMailMessage.IsDraft)
                _changeSendMessageControlsVisibilityAndMessage(Visibility.Visible, SelectedMailMessage);

            if (!SelectedMailMessage.IsSeen && !SelectedMailMessage.IsDraft)
            {
                if (!SelectedMailMessage.IsPopMessage)
                    await _getService().FlagMessageAsync(SelectedMailMessage, MessageFlags.Seen, GetSpecialFolder(SelectedMailMessage.Folder), SelectedMailMessage.Folder);
                SelectedMailMessage.IsSeen = true;
                await _updateMessagesAction(new MailMessage { Uid = SelectedMailMessage.Uid }, SelectedMailMessage);
            }
        }
        #endregion

        #region Changing search box control visibility
        private void ChangeSearchBoxControlVisibility(Visibility visibility) => SearchBoxControlsVisibility = visibility;
        #endregion

        private SpecialFolder GetSpecialFolder(string folder)
        {
            return folder switch
            {
                "" => SpecialFolder.All,
                "Sent" => SpecialFolder.Sent,
                "Flagged" => SpecialFolder.All,
                _ => throw new ArgumentException(folder)
            };
        }
    }
}
