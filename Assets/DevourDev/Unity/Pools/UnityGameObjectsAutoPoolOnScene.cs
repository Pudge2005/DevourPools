using DevourDev.Pools;
using UnityEngine;

namespace DevourDev.Unity.Pools
{
    public abstract class UnityGameObjectsAutoPoolOnScene<T> : MonoBehaviour, IPool<T>, IExpandablePool<T>
        where T : Component, IAutoPoolableItem<T>
    {
        [SerializeField] private T _prefab;
        [SerializeField] private Transform _parent;
        [SerializeField] private int _initialCapacity = 64;
        [SerializeField] private int _maxCapacity = 512;
        [SerializeField] private int _preloadedCount = 32;


        private UnityGameObjectsAutoPool<T> _internalPool;


        private void Awake()
        {
            _internalPool = new(_prefab, _parent,
                _preloadedCount, true,
                _maxCapacity);

            if (_preloadedCount > 0)
                Preload(_preloadedCount);
        }

        public bool AutoExpand { get => ((IExpandablePool<T>)_internalPool).AutoExpand; set => ((IExpandablePool<T>)_internalPool).AutoExpand = value; }
        public int MaxAutoExpandCapacity { get => ((IExpandablePool<T>)_internalPool).MaxAutoExpandCapacity; set => ((IExpandablePool<T>)_internalPool).MaxAutoExpandCapacity = value; }

        public void SetCapacity(int capacity)
        {
            ((IExpandablePool<T>)_internalPool).SetCapacity(capacity);
        }

        public int Count => ((IPool<T>)_internalPool).Count;

        public int Capacity => ((IPool<T>)_internalPool).Capacity;

        public T Rent()
        {
            return ((IPool<T>)_internalPool).Rent();
        }

        public void Return(T rentedItem)
        {
            ((IPool<T>)_internalPool).Return(rentedItem);
        }

        public void SetCount(int count)
        {
            ((IPool<T>)_internalPool).SetCount(count);
        }

        public void Preload(int count)
        {
            ((IPool<T>)_internalPool).Preload(count);
        }

        public void Clear()
        {
            ((IPool<T>)_internalPool).Clear();
        }

        public void Clear(int toRemoveCount)
        {
            ((IPool<T>)_internalPool).Clear(toRemoveCount);
        }
    }
}
