namespace DevourDev.Pools
{
    public interface IExpandablePool<T>
         where T : class
    {
        bool AutoExpand { get; set; }
        int MaxAutoExpandCapacity { get; set; }


        void SetCapacity(int capacity);
    }

}
