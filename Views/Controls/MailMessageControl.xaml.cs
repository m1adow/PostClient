using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PostClient.Views.Controls
{
    internal sealed partial class MailMessageControl : UserControl
    {
        public string Subject
        {
            get => (string)GetValue(SubjectProperty);
            set => SetValue(SubjectProperty, value);
        }

        public DateTimeOffset Date
        {
            get => (DateTimeOffset)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }

        public string From
        {
            get => (string)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public MailMessageControl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty SubjectProperty =
            DependencyProperty.Register(nameof(Subject), typeof(string), typeof(MailMessageControl), new PropertyMetadata(null, OnDependencyPropertyChanged));
        
        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register(nameof(Date), typeof(DateTimeOffset), typeof(MailMessageControl), new PropertyMetadata(null, OnDependencyPropertyChanged));      
        
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register(nameof(From), typeof(string), typeof(MailMessageControl), new PropertyMetadata(null, OnDependencyPropertyChanged));

        private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MailMessageControl control = d as MailMessageControl;

            if (control.subjectTextBlock.Text == string.Empty)
                control.subjectTextBlock.Text = e.NewValue as string;

            if (e.NewValue.GetType() != typeof(string))
                control.dateTextBlock.Text = ((DateTimeOffset)e.NewValue).ToString();

            control.fromTextBlock.Text = (e.NewValue as string) ?? string.Empty;
        } 
    }
}
