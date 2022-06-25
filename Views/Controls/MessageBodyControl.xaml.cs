using PostClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PostClient.Views.Controls
{
    internal sealed partial class MessageBodyControl : UserControl
    {
        public string MessageBody
        {
            get => (string)GetValue(MessageBodyProperty);
            set => SetValue(MessageBodyProperty, value);
        }

        public MessageBodyControl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty MessageBodyProperty =
            DependencyProperty.Register("MessageBody", typeof(string), typeof(MessageBodyControl), new PropertyMetadata(null, SetText));

        private static void SetText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MessageBodyControl control = d as MessageBodyControl;

            string messageBody = e.NewValue as string ?? string.Empty;

            control.webView.NavigateToString(messageBody);
        }
    }
}
