

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Homura.Core
{
    public class NotifyPropertyChangedImpl : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T destination, T value, [CallerMemberName] string caller = "")
        {
            if (Equals(destination, value))
                return false;

            destination = value;
            OnPropertyChanged(caller);
            return true;
        }

        protected void OnPropertyChanged(params string[] propertyNames)
        {
            if (PropertyChanged != null)
            {
                foreach (var propertyName in propertyNames)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
