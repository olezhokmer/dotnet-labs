using System;
using System.Collections;
using System.Collections.Generic;

namespace CustomDictionaryLibrary {
    public class CustomDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private class Node
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public Node Next { get; set; }
        }

        private Node head;

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        public ICollection<TKey> Keys
        {
            get
            {
                List<TKey> keys = new List<TKey>();
                Node current = head;

                while (current != null)
                {
                    keys.Add(current.Key);
                    current = current.Next;
                }

                return keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> values = new List<TValue>();
                Node current = head;

                while (current != null)
                {
                    values.Add(current.Value);
                    current = current.Next;
                }

                return values;
            }
        }

        public TValue this[TKey key]
        {
            get => GetItem(key);
            set
            {
                AddItem(key, value);
            }
        }

        public event EventHandler<ItemAddedEventArgs<TKey, TValue>> ItemAdded;

        public void Add(TKey key, TValue value)
        {
            AddItem(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            AddItem(item.Key, item.Value);
        }

        public void Clear()
        {
            head = null;
            Count = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            Node current = head;

            while (current != null)
            {
                if (EqualityComparer<TKey>.Default.Equals(current.Key, item.Key) &&
                    EqualityComparer<TValue>.Default.Equals(current.Value, item.Value))
                {
                    return true;
                }

                current = current.Next;
            }

            return false;
        }

        public bool ContainsKey(TKey key)
        {
            Node current = head;

            while (current != null)
            {
                if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
                {
                    return true;
                }

                current = current.Next;
            }

            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (array.Length - arrayIndex < Count)
            {
                throw new ArgumentException("The destination array has fewer elements than the collection.");
            }

            Node current = head;
            while (current != null)
            {
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                current = current.Next;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            Node current = head;

            while (current != null)
            {
                yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                current = current.Next;
            }
        }

        public bool Remove(TKey key)
        {
            Node current = head;
            Node previous = null;

            while (current != null)
            {
                if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
                {
                    if (previous == null)
                    {
                        head = current.Next;
                    }
                    else
                    {
                        previous.Next = current.Next;
                    }

                    Count--;
                    return true;
                }

                previous = current;
                current = current.Next;
            }

            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            Node current = head;

            while (current != null)
            {
                if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
                {
                    value = current.Value;
                    return true;
                }

                current = current.Next;
            }

            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual void OnItemAdded(TKey key, TValue value)
        {
            ItemAdded?.Invoke(this, new ItemAddedEventArgs<TKey, TValue>(key, value));
        }

        private void AddItem(TKey key, TValue value)
        {
            Node newNode = new Node { Key = key, Value = value, Next = head };
            head = newNode;

            OnItemAdded(key, value);
            Count++;
        }

        private TValue GetItem(TKey key)
        {
            Node current = head;

            while (current != null)
            {
                if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
                {
                    return current.Value;
                }

                current = current.Next;
            }

            throw new KeyNotFoundException($"Key '{key}' not found in the dictionary.");
        }
    }

    public class ItemAddedEventArgs<TKey, TValue> : EventArgs
    {
        public TKey Key { get; }
        public TValue Value { get; }

        public ItemAddedEventArgs(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}