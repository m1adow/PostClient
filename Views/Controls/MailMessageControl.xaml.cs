using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PostClient.Views.Controls
{
    #nullable enable

    internal sealed partial class MailMessageControl : UserControl
    {
        public string? Subject
        {
            get => (string)GetValue(SubjectProperty);
            set => SetValue(SubjectProperty, value);
        }

        public static readonly DependencyProperty SubjectProperty =
            DependencyProperty.Register(nameof(Subject), typeof(string), typeof(MailMessageControl), new PropertyMetadata(null, OnSubjectDependencyPropertyChanged));

        public DateTimeOffset Date
        {
            get => (DateTimeOffset)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }

        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register(nameof(Date), typeof(DateTimeOffset), typeof(MailMessageControl), new PropertyMetadata(null, OnDateDependencyPropertyChanged));

        public string? From
        {
            get => (string)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register(nameof(From), typeof(string), typeof(MailMessageControl), new PropertyMetadata(null, OnFromDependencyPropertyChanged));

        public MailMessageControl()
        {
            this.InitializeComponent();
        }

        private static void OnSubjectDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MailMessageControl? control = (d as MailMessageControl) ?? new MailMessageControl();

            control.subjectTextBlock.Text = (e.NewValue as string) ?? string.Empty;
        }

        private static void OnDateDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MailMessageControl? control = (d as MailMessageControl) ?? new MailMessageControl();

            control.dateTextBlock.Text = (((DateTimeOffset)e.NewValue).ToString()) ?? new DateTimeOffset().ToString();
        }

        private static void OnFromDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MailMessageControl? control = (d as MailMessageControl) ?? new MailMessageControl();

            control.fromTextBlock.Text = (e.NewValue as string) ?? string.Empty;
        }
    }
}
