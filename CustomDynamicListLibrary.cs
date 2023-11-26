using System;
using System.Collections;
using System.Collections.Generic;

namespace CustomDynamicListLibrary
{
    public class CustomDynamicList<T> : IList<T>
    {
        private T[] items;
        private int count;

        // Event for item added to the collection
        public event EventHandler<ItemAddedEventArgs<T>> ItemAdded;

        // Constructor
        public CustomDynamicList()
        {
            items = new T[1];
            count = 0;
        }

        // Method to add an item to the collection
        public void Add(T item)
        {
            if (count == items.Length)
            {
                Array.Resize(ref items, items.Length * 2);
            }

            items[count] = item;
            count++;

            // Raise the ItemAdded event
            OnItemAdded(new ItemAddedEventArgs<T>(item));
        }

        // Other IList<T> interface methods (e.g., Remove, Contains, Clear, etc.) can be implemented similarly.

        // Event handler method to raise the ItemAdded event
        protected virtual void OnItemAdded(ItemAddedEventArgs<T> e)
        {
            ItemAdded?.Invoke(this, e);
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return Array.IndexOf(items, item, 0, count);
        }

        public void Insert(int index, T item)
        {
            // Implementation for Insert
            // ...
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            // Shift elements to the left to fill the gap
            for (int i = index; i < count - 1; i++)
            {
                items[i] = items[i + 1];
            }

            // Clear the last element and decrement the count
            items[count - 1] = default(T);
            count--;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return items[index];
            }
            set
            {
                if (index < 0 || index >= count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                items[index] = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Clear()
        {
            Array.Clear(items, 0, count);
            count = 0;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(items, 0, array, arrayIndex, count);
        }

        public bool IsReadOnly => false;

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        public int Count => count;

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return items[i];
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    // EventArgs class for item added event
    public class ItemAddedEventArgs<T> : EventArgs
    {
        public T Item { get; private set; }

        public ItemAddedEventArgs(T item)
        {
            Item = item;
        }
    }
}
