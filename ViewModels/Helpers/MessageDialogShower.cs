using System;
using Windows.UI.Popups;

namespace PostClient.ViewModels.Helpers
{
    internal static class MessageDialogShower
    {
        public static async void ShowMessageDialog(string message)
        {
            var messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        }
    }
}
