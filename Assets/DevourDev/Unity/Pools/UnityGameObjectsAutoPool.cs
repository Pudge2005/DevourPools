using DevourDev.Pools;
using UnityEngine;

namespace DevourDev.Unity.Pools
{
    public class UnityGameObjectsAutoPool<T> : UnityGameObjectsPool<T>
      where T : Component, IAutoPoolableItem<T>
    {
        public UnityGameObjectsAutoPool(T prefab, Transform parent) : base(prefab, parent)
        {
        }

        public UnityGameObjectsAutoPool(T prefab, Transform parent, int initialCapacity, bool autoExpand, int maxAutoExpandCapacity) : base(prefab, parent, initialCapacity, autoExpand, maxAutoExpandCapacity)
        {
        }

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
            base.HandleDestroyItem(itemToDestroy);
        }

        protected override void HandleItemRented(T rentedItem)
        {
            rentedItem.OnItemRented(this);
            base.HandleItemRented(rentedItem);
        }

        protected override void HandleItemReturned(T returnedItem)
        {
            returnedItem.OnItemReturned();
            base.HandleItemReturned(returnedItem);
        }
    }
}
