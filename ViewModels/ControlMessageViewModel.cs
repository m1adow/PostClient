using PostClient.ViewModels.Infrastructure;
using System.Net.Mail;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace PostClient.ViewModels
{
    internal sealed class ControlMessageViewModel : ViewModelBase
    {
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

        public ICommand DeleteMessageCommand { get; }

        public ICommand CloseMessageCommand { get; }

        public ControlMessageViewModel()
        {
            DeleteMessageCommand = new RelayCommand(DeleteMessage);
            CloseMessageCommand = new RelayCommand(CloseMessage);
        }

        #region Methods for deleting message
        private void DeleteMessage()
        {
            

            MessageBodyControlsVisibility = Visibility.Collapsed;
        }
        #endregion

        #region Method for closing message
        private void CloseMessage()
        {
            _selectedMailMessage = new MailMessage();

            MessageBodyControlsVisibility = Visibility.Collapsed;
        }
        #endregion
    }
}
