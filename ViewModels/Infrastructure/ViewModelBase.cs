using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PostClient.ViewModels.Infrastructure
{
    internal abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected virtual void Set<T>(ref T field, T value, ICommand[] commands = null, [CallerMemberName] string propertyName = null)
        {
            if (field.Equals(value))
                return;

            field = value;
            OnPropertyChanged(propertyName);

            if (commands != null)
                foreach (var command in commands)
                    (command as RelayCommand).OnExecuteChanged();
        }
    }
}
