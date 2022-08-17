using System;
using Windows.UI.Xaml.Controls;

namespace PostClient.Helpers
{
    public static class ContentDialogShower
    {
        public static async void ShowContentDialog(string title, string content)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    PrimaryButtonText = "OK"
                };
                await dialog.ShowAsync();
            }
            catch { }
        }
    }
}
