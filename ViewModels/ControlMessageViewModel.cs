using MailKit;
using PostClient.Models;
using PostClient.Models.Infrastructure;
using PostClient.ViewModels.Infrastructure;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

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

        private MailMessage? _stableMailMessage = new MailMessage();

        public MailMessage? StableMailMessage
        {
            get => _selectedMailMessage;
            set
            {
                if (value == null)
                    value = new MailMessage();

                Set(ref _stableMailMessage, value);
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

        public ICommand CloseMessageCommand { get; }

        public ICommand HideMessageViewCommand { get; }

        public ICommand ChangeSelectedMessageCommand { get; }

        public ICommand ChangeToTappedStateCommand { get; }

        private readonly Func<MailMessage, Task<bool>> _updateFlaggedList;

        private readonly Func<MailMessage, bool> _deleteMessageFromList;

        private readonly Func<Visibility, MailMessage, bool> _changeSendMessageControlsVisibilityAndMessage;

        private readonly Func<IPostService> _getService;

        public ControlMessageViewModel(Func<IPostService> getService, Func<MailMessage, Task<bool>> updateFlaggedList, Func<MailMessage, bool> deleteMessageFromList, Func<Visibility, MailMessage, bool> changeSendMessageControlsVisibilityAndMessage)
        {
            _getService = getService;
            _updateFlaggedList = updateFlaggedList;
            _deleteMessageFromList = deleteMessageFromList;
            _changeSendMessageControlsVisibilityAndMessage = changeSendMessageControlsVisibilityAndMessage;

            FlagMessageCommand = new RelayCommand(FlagMessage);
            DeleteMessageCommand = new RelayCommand(DeleteMessage);
            CloseMessageCommand = new RelayCommand(CloseMessage);
            HideMessageViewCommand = new RelayCommand(HideMessageView);
            ChangeSelectedMessageCommand = new RelayCommand(ChangeSelectedMessage);
            ChangeToTappedStateCommand = new RelayCommand(ChangeToTappedState);
        }

        #region Flag message
        private async void FlagMessage(object parameter)
        {
            await _getService().FlagMessage(SelectedMailMessage, GetSpecialFolder(SelectedMailMessage.Folder), SelectedMailMessage.Folder);
            await _updateFlaggedList(SelectedMailMessage);
        }
        #endregion

        #region Delete message
        private async void DeleteMessage(object parameter)
        {
            await _getService().DeleteMessage(SelectedMailMessage, GetSpecialFolder(SelectedMailMessage.Folder), SelectedMailMessage.Folder);
            _deleteMessageFromList(SelectedMailMessage);
            CloseMessage(parameter);
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

        #region Change selected message
        private void ChangeSelectedMessage(object parameter) => SelectedMailMessage = parameter as MailMessage;
        #endregion

        #region Change to tapped state
        private void ChangeToTappedState(object parameter)
        {
            SelectedMailMessage = parameter as MailMessage;
           
            if (SelectedMailMessage.Body.Length > 0 && !SelectedMailMessage.IsDraft)
                MessageViewConrtolVisibility = Visibility.Visible;
            else if (SelectedMailMessage.IsDraft)
                _changeSendMessageControlsVisibilityAndMessage(Visibility.Visible, SelectedMailMessage);

            StableMailMessage = SelectedMailMessage;
        } 
        #endregion

        private SpecialFolder GetSpecialFolder(string folder)
        {
            return folder switch
            {
                "" => SpecialFolder.All,
                "Sent" => SpecialFolder.Sent,
                _ => throw new ArgumentException(folder)
            };
        }
    }
}
