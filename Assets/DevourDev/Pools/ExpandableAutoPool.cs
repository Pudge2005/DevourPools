namespace DevourDev.Pools
{
    public abstract class ExpandableAutoPool<T> : ExpandablePoolBase<T>
        where T : class, IAutoPoolableItem<T>
    {
        protected internal override T CreateItemInternal()
        {
            var item = CreateItem();
            item.OnItemCreated(this);
            HandleItemReturned(item);
            return item;
        }

        protected override void HandleDestroyItem(T itemToDestroy)
        {
           itemToDestroy.OnItemDestroyed();
        }

        protected override void HandleItemRented(T rentedItem)
        {
            rentedItem.OnItemRented(this);
        }

        protected override void HandleItemReturned(T returnedItem)
        {
            returnedItem.OnItemReturned();
        }
    }
}
