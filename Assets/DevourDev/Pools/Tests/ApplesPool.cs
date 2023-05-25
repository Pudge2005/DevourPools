namespace DevourDev.Pools.Tests
{
    public sealed class ApplesPool : DevourDev.Pools.ThreadSafePoolBase<Apple>
    {
        public ApplesPool()
        {
        }

        public ApplesPool(int initialCapacity, int initialCount) : base(initialCapacity, initialCount)
        {
        }


        public int TotalRents { get; set; }


        protected override Apple CreateItem()
        {
            return new();
        }

        protected override void HandleItemRented(Apple rentedItem)
        {
            base.HandleItemRented(rentedItem);
            ++TotalRents;
        }
        protected override void HandleItemReturned(Apple returnedItem)
        {
            returnedItem.Name = string.Empty;
            returnedItem.Weight = default;
        }
    }

}
