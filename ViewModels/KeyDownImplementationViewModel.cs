using PostClient.ViewModels.Infrastructure;
using System;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace PostClient.ViewModels
{
    public sealed class KeyDownImplementationViewModel : ViewModelBase
    {
        public ICommand ListViewLoadedCommand { get; }

        private readonly Action<object> _deleteMessage;

        private readonly Action<object> _flagMessage;

        private readonly Action<object> _unseenMessage;

        private readonly Action<object> _archiveMessage;

        private readonly Action<object> _syncMessages;

        public KeyDownImplementationViewModel(Action<object> deleteMessage, Action<object> flagMessage, Action<object> unseenMessage, Action<object> archiveMessage, Action<object> syncMessages)
        {
            _deleteMessage = deleteMessage;
            _flagMessage = flagMessage;
            _unseenMessage = unseenMessage;
            _archiveMessage = archiveMessage;
            _syncMessages = syncMessages;

            ListViewLoadedCommand = new RelayCommand(ListViewLoadedHandler);
        }

        private void ListViewLoadedHandler(object parameter)
        {
            var listView = parameter as ListView;

            if (listView != null)
                listView.KeyDown += ListView_KeyDown;
        }

        private void ListView_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Delete:
                    _deleteMessage(new object());
                    break;
                case VirtualKey.F:
                    _flagMessage(new object());
                    break;
                case VirtualKey.U:
                    _unseenMessage(new object());
                    break;
                case VirtualKey.A:
                    _archiveMessage(new object());
                    break;
                case VirtualKey.R:
                    _syncMessages(new object());
                    break;
            }
        }
    }
}
