using System;
using Windows.UI.Popups;

namespace PostClient.Models.Helpers
{
    public static class MessageDialogShower
    {
        public static async void ShowMessageDialog(string message)
        {
            try
            {
                var messageDialog = new MessageDialog(message);
                await messageDialog.ShowAsync();
            }
            catch { }
        }
    }
}
