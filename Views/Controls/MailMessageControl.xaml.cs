using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

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

        public ICommand FlagCommand
        {
            get => (ICommand)GetValue(FlagCommandProperty);
            set => SetValue(FlagCommandProperty, value);
        }

        public static readonly DependencyProperty FlagCommandProperty =
            DependencyProperty.Register(nameof(FlagCommand), typeof(ICommand), typeof(MailMessageControl), new PropertyMetadata(null, OnFlagCommandDependencyPropertyChanged));

        public ICommand DeleteCommand
        {
            get => (ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(MailMessageControl), new PropertyMetadata(null, OnDeleteCommandDependencyPropertyChanged));


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

        private static void OnFlagCommandDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MailMessageControl? control = (d as MailMessageControl) ?? new MailMessageControl();

            control.flagAppBarButton.Command = e.NewValue as ICommand;
        }

        private static void OnDeleteCommandDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MailMessageControl? control = (d as MailMessageControl) ?? new MailMessageControl();

            control.deleteAppBarButton.Command = e.NewValue as ICommand;
        }

        private void Grid_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            commandBarFlyoutFlagAndDelete.ShowAt(sender as Grid, new FlyoutShowOptions { ShowMode = FlyoutShowMode.Standard });
        }

        private void SwipeItemFlag_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args) => FlagCommand.Execute(new object());

        private void SwipeItemDelete_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args) => DeleteCommand.Execute(new object());
    }
}
