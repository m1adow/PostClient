using PostClient.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PostClient.Views.Controls
{
    internal sealed partial class MailMessageControl : UserControl
    {
        public MailMessage MailMessage
        {
            get => (MailMessage)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public MailMessageControl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("MailMessage", typeof(MailMessage), typeof(MailMessageControl), new PropertyMetadata(null, OnDependencyPropertyChanged));

        private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MailMessageControl control = d as MailMessageControl;

            control.infoGrid.DataContext = (e.NewValue as MailMessage) ?? new MailMessage();
        }
    }
}
