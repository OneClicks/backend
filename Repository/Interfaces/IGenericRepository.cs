using MongoDB.Driver;

namespace backend.Repository.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> Create(T data);
        Task<bool> Exists(FilterDefinition<T> filter);
        Task<T> ExistsOnly(FilterDefinition<T> filter);
        Task<UpdateResult> UpdateOnly(FilterDefinition<T> filter, UpdateDefinition<T> update);
        Task<List<T>> FetchAll();
        Task<bool> RemoveById(FilterDefinition<T> filter);

    }
}
