using DevourDev.Pools;
using UnityEngine;

namespace DevourDev.Unity.Pools.Tests
{
    public sealed class Ball : AutoPoolableUnityItem<Ball>
    {
        [SerializeField] private Rigidbody _rb;


        private void FixedUpdate()
        {
            if (transform.position.y < -10)
                ReturnToPool();
        }


        protected override void HandleItemCreated()
        {
            base.HandleItemCreated();

        }

        protected override void HandleItemDestroyed()
        {
            base.HandleItemDestroyed();
        }

        protected override void HandleItemRented()
        {
            base.HandleItemRented();
        }

        protected override void HandleItemReturned()
        {
            base.HandleItemReturned();
            _rb.Sleep();
        }
    }
}
