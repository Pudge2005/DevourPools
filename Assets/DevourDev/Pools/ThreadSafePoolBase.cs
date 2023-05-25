using System;

namespace DevourDev.Pools
{
    public abstract class ThreadSafePoolBase<T> : IPool<T>, IThreadSafePool<T>
         where T : class
    {
        public const int DefaultBufferSize = 64;

        private readonly object _lockObj = new();

        private readonly T[] _buffer;
        private readonly int _capacity;

        private int _count;


        public ThreadSafePoolBase() : this(DefaultBufferSize, 0)
        {
        }

        public ThreadSafePoolBase(int initialCapacity, int initialCount)
        {
            if (initialCapacity < 0)
            {
                throw new System.ArgumentException($"Capacity can not be negative ({initialCapacity})", nameof(initialCapacity));
            }

            if (initialCount < 0)
            {
                throw new System.ArgumentException($"Count can not be negative ({initialCount})", nameof(initialCount));
            }

            _capacity = initialCapacity;
            _buffer = new T[initialCapacity];

            if (initialCount > 0)
                Preload(initialCount);
        }


        public int Count
        {
            get
            {
                lock (_lockObj)
                {
                    return _count;
                }
            }
        }

        public int Capacity => _capacity;


        public void SetCount(int value)
        {
            lock (_lockObj)
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
        }

        public void Clear()
        {
            lock (_lockObj)
            {
                Clear(_count);
            }
        }

        public void Clear(int countToRemove)
        {
            lock (_lockObj)
            {
                ClearHelper(countToRemove, true);
                _count -= countToRemove;
            }
        }

        public void Preload(int countToAdd)
        {
            lock (_lockObj)
            {
                int desiredCount = _count + countToAdd;

                if (desiredCount > _capacity)
                {
                    desiredCount = _capacity;
                }

                countToAdd = desiredCount - _count;
                var span = _buffer.AsSpan(_count, countToAdd);

                for (int i = 0; i < countToAdd; i++)
                {
                    span[i] = CreateItemInternal();
                }

                _count = desiredCount;
            }
        }

        public T Rent()
        {
            lock (_lockObj)
            {
                T item = null;

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
        }

        public void Return(T rentedItem)
        {
            if (rentedItem == null)
            {
                throw new ArgumentNullException(nameof(rentedItem));
            }

            lock (_lockObj)
            {
                int desiredCount = _count + 1;

                if (desiredCount >= _capacity)
                {
                    HandleDestroyItem(rentedItem);
                    return;
                }

                _buffer[_count++] = rentedItem;
                HandleItemReturned(rentedItem);
            }
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


        private T CreateItemInternal()
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

        protected virtual void HandleItemReturned(T rentedItem)
        { }

    }
}
