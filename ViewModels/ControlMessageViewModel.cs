using MailKit;
using PostClient.Models;
using PostClient.Models.Infrastructure;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

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

        public ICommand FlagMessageCommand { get; }

        public ICommand DeleteMessageCommand { get; }

        public ICommand ArchiveMessageCommand { get; }

        public ICommand UnseenMessageCommand { get; }

        public ICommand CloseMessageCommand { get; }

        public ICommand HideMessageViewCommand { get; }

        public ICommand ChangeMessageOnRightTapCommand { get; }

        public ICommand ChangeMessageOnTapCommand { get; }

        private readonly Func<MailMessage, Task<bool>> _updateFlaggedList;

        private readonly Func<MailMessage, bool> _deleteMessageFromList;

        private readonly Func<Visibility, MailMessage, bool> _changeSendMessageControlsVisibilityAndMessage;

        private readonly Func<IPostService> _getService;

        private readonly Action<MailMessage> _archiveMessageAction;

        private readonly Action<MailMessage, MailMessage> _updateMessagesAction;

        public ControlMessageViewModel(Func<IPostService> getService, Func<MailMessage, Task<bool>> updateFlaggedList, Func<MailMessage, bool> deleteMessageFromList, Func<Visibility, MailMessage, bool> changeSendMessageControlsVisibilityAndMessage, Action<MailMessage> archiveMessageAction, Action<MailMessage, MailMessage> updateMessagesAction)
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
            ChangeMessageOnTapCommand = new RelayCommand(ChangeMessageOnTap);
        }

        #region Flag message
        private async void FlagMessage(object parameter)
        {
            await _getService().FlagMessage(SelectedMailMessage, MessageFlags.Flagged, GetSpecialFolder(SelectedMailMessage.Folder), SelectedMailMessage.Folder);
            await _updateFlaggedList(SelectedMailMessage);
        }
        #endregion

        #region Delete message
        private async void DeleteMessage(object parameter)
        {
            await _getService().FlagMessage(SelectedMailMessage, MessageFlags.Deleted, GetSpecialFolder(SelectedMailMessage.Folder), SelectedMailMessage.Folder);
            _deleteMessageFromList(SelectedMailMessage);
            CloseMessage(parameter);
        }
        #endregion

        #region Archive message
        private void ArchiveMessage(object parameter) => _archiveMessageAction(SelectedMailMessage);
        #endregion

        #region Unseen message
        private async void UnseenMessage(object parameter)
        {
            if (SelectedMailMessage.IsSeen)
            {
                await _getService().FlagMessage(SelectedMailMessage, MessageFlags.Seen, GetSpecialFolder(SelectedMailMessage.Folder), SelectedMailMessage.Folder);
                SelectedMailMessage.IsSeen = false;
                _updateMessagesAction(new MailMessage { Uid = SelectedMailMessage.Uid }, SelectedMailMessage);
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

        #region Change message on tap
        private async void ChangeMessageOnTap(object parameter)
        {
            SelectedMailMessage = parameter as MailMessage;

            if (SelectedMailMessage.Body.Length > 0 && !SelectedMailMessage.IsDraft)
                MessageViewConrtolVisibility = Visibility.Visible;
            else if (SelectedMailMessage.IsDraft)
                _changeSendMessageControlsVisibilityAndMessage(Visibility.Visible, SelectedMailMessage);

            if (!SelectedMailMessage.IsSeen)
            {
                await _getService().FlagMessage(SelectedMailMessage, MessageFlags.Seen, GetSpecialFolder(SelectedMailMessage.Folder), SelectedMailMessage.Folder);
                SelectedMailMessage.IsSeen = true;
                _updateMessagesAction(new MailMessage { Uid = SelectedMailMessage.Uid }, SelectedMailMessage);
            }
        }
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
