using UnityEngine;

namespace DevourDev.Unity.Pools
{

    public class UnityComponentsPool<T> : ExpandableUnityObjectsPoolBase<T>
        where T : Component
    {
        protected override T CreateItem()
        {
            return new GameObject("Pooled Component").AddComponent<T>();
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
