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
            DependencyProperty.Register("MailMessage", typeof(MailMessage), typeof(MailMessageControl), new PropertyMetadata(null, SetText));

        private static void SetText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MailMessageControl control = d as MailMessageControl;

            MailMessage mailMessage = e.NewValue as MailMessage; ;

            control.subjectTextBlock.Text = mailMessage.Subject;
            control.dateTextBlock.Text = mailMessage.Date.ToLocalTime().ToString();
            control.fromTextBlock.Text = mailMessage.From;
        }
    }
}
