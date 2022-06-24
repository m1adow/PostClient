using MimeKit;
using System;
using Windows.UI.Popups;

namespace PostClient.Models.Services
{
    public abstract class PostService
    {
        protected async void ShowMessageDialogForException(Exception exception)
        {
            MessageDialog messageDialog = new MessageDialog(exception.Message);
            await messageDialog.ShowAsync();
        }
    }
}
