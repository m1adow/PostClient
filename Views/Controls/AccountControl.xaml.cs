using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PostClient.Views.Controls
{
#nullable enable

    internal sealed partial class AccountControl : UserControl
    {
        public string? AccountName
        {
            get => (string)GetValue(AccountNameProperty);
            set => SetValue(AccountNameProperty, value);
        }

        public static readonly DependencyProperty AccountNameProperty =
            DependencyProperty.Register(nameof(AccountName), typeof(string), typeof(AccountControl), new PropertyMetadata(null, OnAccountNameDependencyPropertyChanged));

        public string? AccountService
        {
            get => (string)GetValue(AccountServiceProperty);
            set => SetValue(AccountServiceProperty, value);
        }

        public static readonly DependencyProperty AccountServiceProperty =
            DependencyProperty.Register(nameof(AccountService), typeof(string), typeof(AccountControl), new PropertyMetadata(null, OnAccountServiceDependencyPropertyChanged));


        public AccountControl()
        {
            this.InitializeComponent();
        }

        private static void OnAccountNameDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as AccountControl;
            if (control != null)
            {
                var name = e.NewValue as string;
                if (name != null)
                {
                    control.NameTextBlock.Text = name;
                }             
            }
        }

        private static void OnAccountServiceDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as AccountControl;
            if (control != null)
            {
                var service = e.NewValue as string;
                if (service != null)
                {
                    control.ServiceTextBlock.Text = service;
                }
            }
        }
    }
}
