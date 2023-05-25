namespace DevourDev.Pools
{
    public interface IPool<T>
        where T : class
    {
        /// <summary>
        /// Get: returns current items count in pool,
        /// ready to be rented.
        /// Set: adds or removes items in pool to
        /// fit provided count.
        /// </summary>
        int Count { get; }
        int Capacity { get; }


        T Rent();

        void Return(T rentedItem);

        void SetCount(int count);

        void Preload(int count);

        void Clear();

        void Clear(int toRemoveCount);
    }

}
