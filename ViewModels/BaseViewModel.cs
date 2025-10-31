using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClassScheduleApp.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Set<T>(ref T backing, T value, [CallerMemberName] string? name = null)
        { if (!Equals(backing, value)) { backing = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); } }
    }
}
