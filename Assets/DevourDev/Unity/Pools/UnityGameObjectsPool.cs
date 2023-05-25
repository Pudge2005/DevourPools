using System;
using UnityEngine;

namespace DevourDev.Unity.Pools
{


    public class UnityGameObjectsPool<T> : ExpandableUnityObjectsPoolBase<T>
      where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;


        public UnityGameObjectsPool(T prefab, Transform parent) : base()
        {
            _prefab = prefab;
            _parent = parent;
        }

        public UnityGameObjectsPool(T prefab, Transform parent, int initialCapacity,
            bool autoExpand, int maxAutoExpandCapacity)
            : base(initialCapacity, autoExpand, maxAutoExpandCapacity)
        {
            _prefab = prefab;
            _parent = parent;
        }


        protected override T CreateItem()
        {
            return GameObject.Instantiate(_prefab, _parent);
        }

        protected override void HandleDestroyItem(T itemToDestroy)
        {
            GameObject.Destroy(itemToDestroy.gameObject);
        }

        protected override void HandleItemRented(T rentedItem)
        {
            rentedItem.gameObject.SetActive(true);
        }

        protected override void HandleItemReturned(T returnedItem)
        {
            returnedItem.gameObject.SetActive(false);
        }
    }
}
