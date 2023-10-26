using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7zip.ViewModels
{
    internal class MultiThreadBindableBase : ObservableObject
    {
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            bool hasThreadAccess = App.MainDispatcherQueue.HasThreadAccess;
            if (hasThreadAccess)
                base.OnPropertyChanged(e);
            else App.MainDispatcherQueue.TryEnqueue(() => base.OnPropertyChanged(e));
        }

        protected override void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            bool hasThreadAccess = App.MainDispatcherQueue.HasThreadAccess;
            if (hasThreadAccess)
                base.OnPropertyChanging(e);
            else App.MainDispatcherQueue.TryEnqueue(() => base.OnPropertyChanging(e));
        }
    }
}
