using System;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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

        public ICommand SendWithDelayCommand
        {
            get => (ICommand)GetValue(SendWithDelayCommandProprerty);
            set => SetValue(SendWithDelayCommandProprerty, value);
        }

        public static readonly DependencyProperty SendWithDelayCommandProprerty =
            DependencyProperty.Register(nameof(SendWithDelayCommand), typeof(ICommand), typeof(CreateMessageControl), new PropertyMetadata(null, OnSendWithDelayCommandDependencyPropertyChanged));

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
            DependencyProperty.Register(nameof(MessageText), typeof(string), typeof(CreateMessageControl), new PropertyMetadata(null, OnMessageTextDependencyPropertyChanged));

        public List<KeyValuePair<string, byte[]>>? Attachments
        {
            get => (List<KeyValuePair<string, byte[]>>)GetValue(AttachmentsProperty);
            set => SetValue(AttachmentsProperty, value);
        }

        public static readonly DependencyProperty AttachmentsProperty =
            DependencyProperty.Register(nameof(Attachments), typeof(List<KeyValuePair<string, byte[]>>), typeof(CreateMessageControl), new PropertyMetadata(null, OnAttachmentsDependecyPropertyChanged));

        public TimeSpan DelayTime
        {
            get => (TimeSpan)GetValue(DelayTimeProperty);
            set => SetValue(DelayTimeProperty, value);
        }

        public static readonly DependencyProperty DelayTimeProperty =
            DependencyProperty.Register(nameof(DelayTime), typeof(TimeSpan), typeof(CreateMessageControl), new PropertyMetadata(null, OnDelayTimeDependecyPropertyChanged));

        private Color _currentColor = Colors.Black;

        public CreateMessageControl()
        {
            this.InitializeComponent();

            Editor.Document.Selection.CharacterFormat.ForegroundColor = _currentColor;
        }

        private static void OnSendCommandDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => AddCommandToButton("Send", d, e);

        private static void OnSendWithDelayCommandDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => AddCommandToButton("SendWithDelay", d, e);

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
                case "SendWithDelay":
                    control.SendWithDelayButton.Command = e.NewValue as ICommand;
                    control.SendWithDelayButton.CommandParameter = control.FilesComboBox;
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

            var text = e.NewValue as string;

            control.MessageText = text;

            control.Editor.Document.GetText(TextGetOptions.UseCrlf, out string documentText);
            if (documentText == "" || text == "")
                control.Editor.Document.SetText(TextSetOptions.FormatRtf, text);
        }

        private static void OnAttachmentsDependecyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CreateMessageControl? control = (d as CreateMessageControl) ?? new CreateMessageControl();

            var files = e.NewValue as List<KeyValuePair<string, byte[]>>;

            control.Attachments = files;
            control.FilesComboBox.Items.Clear();
            files?.ForEach(f => control.FilesComboBox.Items.Add(f.Key));
        }

        private static void OnDelayTimeDependecyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CreateMessageControl? control = (d as CreateMessageControl) ?? new CreateMessageControl();

            control.DelayTime = (TimeSpan)e.NewValue;
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

        private void BackColorButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedColor = (Button)sender;
            var rectangle = (Rectangle)clickedColor.Content;
            var color = ((SolidColorBrush)rectangle.Fill).Color;

            Editor.Document.Selection.CharacterFormat.BackgroundColor = color;

            Editor.Focus(FocusState.Keyboard);
        }

        private void Editor_TextChanged(object sender, RoutedEventArgs e)
        {
            if (Editor.Document.Selection.CharacterFormat.ForegroundColor != _currentColor)
                Editor.Document.Selection.CharacterFormat.ForegroundColor = _currentColor;

            Editor.Document.GetText(TextGetOptions.FormatRtf, out string text);
            MessageText = text;
        }

        private void ColorButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            var border = (Border)sender.Content;
            var color = ((SolidColorBrush)border.Background).Color;

            Editor.Document.Selection.CharacterFormat.ForegroundColor = color;
            _currentColor = color;
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var rect = (Rectangle)e.ClickedItem;
            var color = ((SolidColorBrush)rect.Fill).Color;
            Editor.Document.Selection.CharacterFormat.ForegroundColor = color;
            CurrentColor.Background = new SolidColorBrush(color);

            ColorButton.Flyout.Hide();
            _currentColor = color;
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Insert:
                    SendCommand.Execute(new object());
                    break;
                case VirtualKey.End:
                    CancelCommand.Execute(new object());
                    break;
                case VirtualKey.Home:
                    DraftCommand.Execute(new object());
                    break;
            }
        }

        private void TimePicker_SelectedTimeChanged(TimePicker sender, TimePickerSelectedValueChangedEventArgs args) => DelayTime = sender.Time;
    }
}
