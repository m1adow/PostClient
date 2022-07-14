using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PostClient.Views.Controls
{
    #nullable enable

    internal sealed partial class MessageBodyControl : UserControl
    {
        public string? MessageBody
        {
            get => (string)GetValue(MessageBodyProperty);
            set => SetValue(MessageBodyProperty, value);
        }

        public static readonly DependencyProperty MessageBodyProperty =
            DependencyProperty.Register(nameof(MessageBody), typeof(string), typeof(MessageBodyControl), new PropertyMetadata(null, SetText));


        public List<KeyValuePair<string, byte[]>>? Attachments
        {
            get => (List<KeyValuePair<string, byte[]>>)GetValue(AttachmentsProperty);
            set => SetValue(AttachmentsProperty, value);
        }

        public static readonly DependencyProperty AttachmentsProperty =
            DependencyProperty.Register(nameof(Attachments), typeof(List<KeyValuePair<string, byte[]>>), typeof(MessageBodyControl), new PropertyMetadata(null, AddAttachments));

        public MessageBodyControl()
        {
            this.InitializeComponent();
        }
    
        private static void SetText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MessageBodyControl? control = d as MessageBodyControl;

            string messageBody = e.NewValue as string ?? string.Empty;

            control?.webView.NavigateToString(messageBody);
        }

        private static void AddAttachments(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MessageBodyControl? control = (d as MessageBodyControl) ?? new MessageBodyControl();

            var attachments = (e.NewValue as List<KeyValuePair<string, byte[]>>) ?? new List<KeyValuePair<string, byte[]>>();

            control.attachmentsComboBox.Items.Clear();

            if (attachments.Count == 0)
                control.attachmentsComboBox.Visibility = Visibility.Collapsed;
            else
            {
                control.attachmentsComboBox.Visibility = Visibility.Visible;
                control.attachmentsComboBox.Items.Add("None");

                foreach (var attachment in attachments)
                    control.attachmentsComboBox.Items.Add(attachment.Key);
            }
        }

        private async void AttachmentsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string? fileName = (sender as ComboBox)?.SelectedItem?.ToString();
            
            if (fileName != "None" && fileName != null)
            {
                byte[] attachment = Attachments.FirstOrDefault(a => a.Key == fileName).Value;

                StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Attachments", CreationCollisionOption.ReplaceExisting);
                StorageFile file = await storageFolder.CreateFileAsync(fileName,
                        CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteBytesAsync(file, attachment);

                await Launcher.LaunchFileAsync(file);
            }
        }
    }
}
