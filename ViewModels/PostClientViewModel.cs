﻿using PostClient.ViewModels.Infrastructure;
using System.Windows.Input;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PostClient.Views;

namespace PostClient.ViewModels
{
    public class PostClientViewModel : ViewModelBase
    {
        public ICommand SendCommand { get; private set; }

        public ICommand LoginCommand { get; private set; }

        public PostClientViewModel()
        {
            SendCommand = new RelayCommand(OpenSendMessagePage);
            LoginCommand = new RelayCommand(OpenLoginPage);
        }

        #region Methods for send command
        private void OpenSendMessagePage() => ShowPage(nameof(SendMessagePage));
        #endregion

        #region Methods for login command
        private void OpenLoginPage() => ShowPage(nameof(LoginPage));
        #endregion

        private async void ShowPage(string page)
        {
            var view = CoreApplication.CreateNewView();

            int viewId = 0;

            await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    Frame frame = new Frame();

                    if (page == nameof(SendMessagePage))
                        frame.Navigate(typeof(SendMessagePage));
                    else if (page == nameof(LoginPage))
                        frame.Navigate(typeof(LoginPage));

                    Window.Current.Content = frame;
                    Window.Current.Activate();

                    viewId = ApplicationView.GetForCurrentView().Id;
                });

            await ApplicationViewSwitcher.TryShowAsStandaloneAsync(viewId, ViewSizePreference.UseMinimum);
        }
    }
}
