using System;

namespace DevourDev.Pools
{
    public abstract class ExpandablePoolBase<T> : IPool<T>, IExpandablePool<T>
       where T : class
    {
        public const bool DefaultAutoExpand = true;
        public const int DefaultMaxAutoExpandCapacity = 256;

        private static readonly T[] _emptyArray = new T[0];

        private T[] _buffer;
        private int _count;
        private int _capacity;
        private bool _autoExpand;
        private int _maxAutoExpandCapacity;


        public ExpandablePoolBase()
            : this(0, 0, DefaultAutoExpand, DefaultMaxAutoExpandCapacity)
        {
        }

        public ExpandablePoolBase(int initialCapacity, int initialCount)
            : this(initialCapacity, initialCount, false, 0)
        {
        }

        public ExpandablePoolBase(int initialCapacity, int initialCount, bool autoExpand, int maxAutoExpandCapacity)
        {
            if (initialCapacity < 0 || initialCount < 0)
            {
                throw new System.ArgumentException($"{nameof(initialCapacity)} < 0 or {nameof(initialCount)} < 0 " +
                    $"({initialCapacity}, {initialCount}).");
            }

            if (initialCapacity < initialCount)
            {
                throw new System.ArgumentException($"{nameof(initialCapacity)} < {nameof(initialCount)} " +
                    $"({initialCapacity} < {initialCount}).");
            }

            _buffer = initialCapacity == 0 ? _emptyArray : new T[initialCapacity];
            _capacity = initialCapacity;

            if (initialCount > 0)
            {
                Preload(initialCount);
            }

            if (autoExpand)
            {
                _autoExpand = autoExpand;
                _maxAutoExpandCapacity = maxAutoExpandCapacity > initialCapacity ? maxAutoExpandCapacity : initialCapacity;
            }

        }


        public int Count => _count;

        public int Capacity => _capacity;

        public bool AutoExpand
        {
            get => _autoExpand;
            set => _autoExpand = value;
        }

        public int MaxAutoExpandCapacity
        {
            get => _maxAutoExpandCapacity;
            set => _maxAutoExpandCapacity = value;
        }


        public void SetCount(int value)
        {
            int delta = value - _count;

            if (delta > 0)
            {
                Preload(delta);
            }
            else if (delta < 0)
            {
                Clear(-delta);
            }
        }

        public void SetCapacity(int value)
        {
            if (value < 0)
            {
                throw new ArgumentException($"Attempt to set negative capacity ({value})", nameof(value));
            }

            if (value == _capacity)
                return;

            T[] prevArr = _buffer;
            T[] newArr = value == 0 ? _emptyArray : new T[value];
            int count = _count;

            if (value < count)
            {
                ClearHelper(count - value, false);
                count = value;
            }

            Array.Copy(prevArr, newArr, count);
            _buffer = newArr;
            _capacity = value;
            _count = count;
        }


        public void Clear()
        {
            Clear(_count);
        }

        public void Clear(int countToRemove)
        {
            ClearHelper(countToRemove, true);
            _count -= countToRemove;
        }

        public void Preload(int countToAdd)
        {
            int desiredCount = _count + countToAdd;

            if (desiredCount > _capacity)
            {
                if (!_autoExpand)
                {
                    desiredCount = _capacity;
                }
                else
                {
                    if (desiredCount > _maxAutoExpandCapacity)
                    {
                        desiredCount = _maxAutoExpandCapacity;
                    }

                    EnsureCapacity(desiredCount);
                }
            }

            countToAdd = desiredCount - _count;
            var span = _buffer.AsSpan(_count, countToAdd);

            for (int i = 0; i < countToAdd; i++)
            {
                span[i] = CreateItemInternal();
            }

            _count = desiredCount;
        }

        public T Rent()
        {
            T item;
            if (_count == 0)
            {
                item = CreateItemInternal();
            }
            else
            {
                item = _buffer[--_count];
            }

            HandleItemRented(item);
            return item;
        }

        public void Return(T rentedItem)
        {
            if (rentedItem is null)
            {
                throw new ArgumentNullException(nameof(rentedItem));
            }

            int desiredCount = _count + 1;

            if (desiredCount > _capacity)
            {
                if (_autoExpand && _maxAutoExpandCapacity >= desiredCount)
                {
                    EnsureCapacity(desiredCount);
                }
                else
                {
                    HandleDestroyItem(rentedItem);
                    return;
                }
            }

            _buffer[_count++] = rentedItem;
            HandleItemReturned(rentedItem);
        }


        private void ClearHelper(int count, bool clearBuffer)
        {
            var span = _buffer.AsSpan(_count - count, count);

            for (int i = 0; i < count; i++)
            {
                HandleDestroyItem(span[i]);
            }

            if (clearBuffer)
            {
                span.Fill(null);
            }
        }

        private void EnsureCapacity(int desiredCount)
        {
            const int terminalCapacity = 2146435071 / 2;

            int num = _capacity < 8 ? 8 : _capacity > terminalCapacity ? terminalCapacity : _capacity * 2;

            if (num < desiredCount)
                num = desiredCount;

            SetCapacity(num);
        }

        protected internal virtual T CreateItemInternal()
        {
            var item = CreateItem();
            HandleItemReturned(item);
            return item;
        }

        protected abstract T CreateItem();

        protected virtual void HandleDestroyItem(T itemToDestroy)
        { }

        protected virtual void HandleItemRented(T rentedItem)
        { }

        protected virtual void HandleItemReturned(T returnedItem)
        { }
    }
}
