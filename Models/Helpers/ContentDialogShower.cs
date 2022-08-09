﻿using System;
using Windows.UI.Xaml.Controls;

namespace PostClient.Models.Helpers
{
    public static class ContentDialogShower
    {
        public static async void ShowMessageDialog(string title, string content)
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
