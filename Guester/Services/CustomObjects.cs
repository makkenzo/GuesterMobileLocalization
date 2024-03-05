using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester.Services
{
    public class CustomObservableCollection<T> : ObservableRangeCollection<T>
    {
        public event EventHandler<T> ItemAdded;
        public event EventHandler<T> ItemRemoved;

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            ItemAdded?.Invoke(this, item);
        }

        protected override void RemoveItem(int index)
        {
            T removedItem = this[index];
            base.RemoveItem(index);
            ItemRemoved?.Invoke(this, removedItem);
        }
    }
}
