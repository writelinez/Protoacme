using System.Threading.Tasks;

namespace Protoacme.Core.Abstractions
{
    public interface ICachedRepository<TModel>
    {
        Task<TModel> GetAsync();
        void Update(TModel model);
    }
}