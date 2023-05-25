using DevourDev.Pools;
using UnityEngine;

namespace DevourDev.Unity.Pools
{
    public abstract class AutoPoolableUnityItem<T> : MonoBehaviour, IAutoPoolableItem<T>
        where T : AutoPoolableUnityItem<T>
    {
        private IPool<T> _pool;
        private bool _inPool;


        void IAutoPoolableItem<T>.OnItemCreated(IPool<T> pool)
        {
            _pool = pool;
            HandleItemCreated();
        }

        void IAutoPoolableItem<T>.OnItemDestroyed()
        {
            HandleItemDestroyed();
        }
          

        void IAutoPoolableItem<T>.OnItemRented(IPool<T> pool)
        {
#if UNITY_EDITOR
            if (!_inPool)
                throw new System.Exception($"attempt to rent item that is not in pool");
#endif
            _inPool = false;
            HandleItemRented();
        }

        void IAutoPoolableItem<T>.OnItemReturned()
        {
#if UNITY_EDITOR
            if (_inPool)
                throw new System.Exception($"({nameof(IAutoPoolableItem<T>.OnItemReturned)}) attempt to return item that is pool");
#endif

            _inPool = true;
            HandleItemReturned();
        }

        public void ReturnToPool()
        {
#if UNITY_EDITOR
            if (_inPool)
                throw new System.Exception($"({nameof(IAutoPoolableItem<T>.ReturnToPool)}) attempt to return item that is pool");
#endif
            _pool.Return((T)this);
        }


        protected virtual void HandleItemCreated()
        {

        }

        protected virtual void HandleItemDestroyed()
        {
        }

        protected virtual void HandleItemReturned()
        {

        }

        protected virtual void HandleItemRented()
        {

        }
    }
}
