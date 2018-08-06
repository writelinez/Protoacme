using System.Threading.Tasks;

namespace Protoacme.Core.InternalRepositories
{
    internal interface ICachedRepository<TModel>
    {
        Task<TModel> GetAsync();
        void Update(TModel model);
    }
}