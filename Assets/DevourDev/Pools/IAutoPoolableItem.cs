namespace DevourDev.Pools
{
    public interface IAutoPoolableItem<T>
        where T : class, IAutoPoolableItem<T>
    {
        void OnItemCreated(IPool<T> pool);

        void OnItemDestroyed();

        void OnItemRented(IPool<T> pool);

        void OnItemReturned();


        void ReturnToPool();
    }
}
