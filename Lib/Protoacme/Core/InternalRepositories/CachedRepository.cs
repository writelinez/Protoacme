using Protoacme.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Protoacme.Core.InternalRepositories
{
    internal class CachedRepository<TModel> : ICachedRepository<TModel>
    {
        private readonly Func<Task<TModel>> _sourceFunc;

        private TModel Model;

        public CachedRepository(Func<Task<TModel>> sourceFunc)
        {
            _sourceFunc = sourceFunc;
        }

        public async Task<TModel> GetAsync()
        {
            if (Model == null)
            {
                Model = await _sourceFunc();
            }
            return Model;
        }

        public void Update(TModel model)
        {
            Model = model;
        }
    }
}
