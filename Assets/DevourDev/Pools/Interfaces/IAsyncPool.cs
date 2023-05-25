using System.Threading.Tasks;

namespace DevourDev.Pools
{
    public interface IAsyncPool<T> : IPool<T>
        where T : class
    {
        Task<T> RentAsync();
    }

}
