using System.Windows.Input;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PostClient.Views.Controls
{

#nullable enable

    public sealed partial class CreateMessageControl : UserControl
    {
        public ICommand SendCommand
        {
            get => (ICommand)GetValue(SendCommandProperty);
            set => SetValue(SendCommandProperty, value);
        }

        public static readonly DependencyProperty SendCommandProperty =
            DependencyProperty.Register(nameof(SendCommand), typeof(ICommand), typeof(CreateMessageControl), new PropertyMetadata(null, OnSendCommandDependencyPropertyChanged));

        public ICommand DraftCommand
        {
            get => (ICommand)GetValue(DraftCommandProperty);
            set => SetValue(DraftCommandProperty, value);
        }

        public static readonly DependencyProperty DraftCommandProperty =
            DependencyProperty.Register(nameof(DraftCommand), typeof(ICommand), typeof(CreateMessageControl), new PropertyMetadata(null, OnDraftCommandDependencyPropertyChanged));

        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProprerty);
            set => SetValue(CancelCommandProprerty, value);
        }

        public static readonly DependencyProperty CancelCommandProprerty =
            DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(CreateMessageControl), new PropertyMetadata(null, OnCancelCommandDependencyPropertyChanged));

        public ICommand AttachCommand
        {
            get => (ICommand)GetValue(AttachCommandProprerty);
            set => SetValue(AttachCommandProprerty, value);
        }

        public static readonly DependencyProperty AttachCommandProprerty =
            DependencyProperty.Register(nameof(AttachCommand), typeof(ICommand), typeof(CreateMessageControl), new PropertyMetadata(null, OnAttachCommandDependencyPropertyChanged));

        public string? MessageText
        {
            get => (string)GetValue(MessageTextProperty);
            set => SetValue(MessageTextProperty, value);
        }

        public static readonly DependencyProperty MessageTextProperty =
            DependencyProperty.Register(nameof(MessageText), typeof(string), typeof(CreateMessageControl), new PropertyMetadata("Hi world!", OnMessageTextDependencyPropertyChanged));

        public CreateMessageControl()
        {
            this.InitializeComponent();

            Editor.Document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(0, 0, 0, 0);
        }

        private static void OnSendCommandDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => AddCommandToButton("Send", d, e);

        private static void OnDraftCommandDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => AddCommandToButton("Draft", d, e);

        private static void OnCancelCommandDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => AddCommandToButton("Cancel", d, e);

        private static void OnAttachCommandDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => AddCommandToButton("Attach", d, e);

        private static void AddCommandToButton(string buttonName, DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CreateMessageControl? control = (d as CreateMessageControl) ?? new CreateMessageControl();

            switch (buttonName)
            {
                case "Send":
                    control.SendButton.Command = e.NewValue as ICommand;
                    control.SendButton.CommandParameter = control.FilesComboBox;
                    break;
                case "Draft":
                    control.DraftButton.Command = e.NewValue as ICommand;
                    control.SendButton.CommandParameter = control.FilesComboBox;
                    break;
                case "Cancel":
                    control.CancelButton.Command = e.NewValue as ICommand;
                    control.SendButton.CommandParameter = control.FilesComboBox;
                    break;
                case "Attach":
                    control.AttachButton.Command = e.NewValue as ICommand;
                    control.AttachButton.CommandParameter = control.FilesComboBox;
                    break;
            }
        }

        private static void OnMessageTextDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CreateMessageControl? control = (d as CreateMessageControl) ?? new CreateMessageControl();

            control.MessageText = e.NewValue as string;
        }

        private void StyleButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (AppBarButton)sender;
            var text = ((TextBlock)button.Content).Text;

            switch (text)
            {
                case "Bold":
                    Editor.Document.Selection.CharacterFormat.Bold = FormatEffect.Toggle;
                    break;
                case "Italic":
                    Editor.Document.Selection.CharacterFormat.Italic = FormatEffect.Toggle;
                    break;
                case "Underline":
                    if (Editor.Document.Selection.CharacterFormat.Underline != UnderlineType.Single)
                        Editor.Document.Selection.CharacterFormat.Underline = UnderlineType.Single;
                    else
                        Editor.Document.Selection.CharacterFormat.Underline = UnderlineType.None;
                    break;
            }
        }

        private void BackColorButton_Click(object sender, RoutedEventArgs e) => ChangeColor(sender, false);

        private void FontColorButton_Click(object sender, RoutedEventArgs e) => ChangeColor(sender, true);

        private void ChangeColor(object sender, bool isFont)
        {
            Button clickedColor = (Button)sender;
            var rectangle = (Rectangle)clickedColor.Content;
            var color = ((SolidColorBrush)rectangle.Fill).Color;

            if (isFont)
                Editor.Document.Selection.CharacterFormat.ForegroundColor = color;
            else
                Editor.Document.Selection.CharacterFormat.BackgroundColor = color;

            Editor.Document.ApplyDisplayUpdates();
            FontColorButton.Flyout.Hide();
            Editor.Focus(FocusState.Keyboard);
        }

        private void Editor_TextChanged(object sender, RoutedEventArgs e)
        {
            Editor.Document.GetText(TextGetOptions.FormatRtf, out string text);
            MessageText = text;
        }
    }
}
