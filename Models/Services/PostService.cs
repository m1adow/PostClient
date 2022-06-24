using MimeKit;
using System;
using Windows.UI.Popups;

namespace PostClient.Models.Services
{
    internal abstract class PostService
    {
        protected async void ShowMessageDialogForException(Exception exception)
        {
            MessageDialog messageDialog = new MessageDialog(exception.Message);
            await messageDialog.ShowAsync();
        }

        protected void CheckForOutOfBounds(int last, int max, int first)
        {
            if (last > max)
                throw new Exception("You've reached last list of messages");
            if (first < 0)
                throw new Exception("You've reached first list of messages");
        }
    }
}
