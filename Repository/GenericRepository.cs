using backend.Repository.Interfaces;
using MongoDB.Driver;

namespace backend.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _mongoCollection;
        private readonly MongoClient _mongoClient;
        public GenericRepository()
        {
            // TODO using values from app.config
            _mongoClient = new MongoClient(
                "mongodb+srv://k201625:rJ2Z0DBSVlDIIuMQ@oneclickscluster.2ibs3gr.mongodb.net/?retryWrites=true&w=majority");

            var mongoDatabase = _mongoClient.GetDatabase(
                "OneClicks");
            _mongoCollection = mongoDatabase.GetCollection<T>(GetCollectionName(typeof(T)));

        }
        private protected string GetCollectionName(Type documentType)
        {
            return documentType.Name;
        }
        public async Task<T> Create(T data)
        {
            await _mongoCollection.InsertOneAsync(data);
            return data;
        }

        public async Task<bool> Exists(FilterDefinition<T> filter)
        {
            return await _mongoCollection.Find(filter).AnyAsync();
        }

        public async Task<T> ExistsOnly(FilterDefinition<T> filter)
        {
            return await _mongoCollection.Find(filter).SingleOrDefaultAsync();

        }

        public async Task<UpdateResult> UpdateOnly(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            return await _mongoCollection.UpdateOneAsync(filter, update);
        }
        public async Task<bool> RemoveById(FilterDefinition<T> filter)
        {
            var delete = await _mongoCollection.DeleteOneAsync(filter);
            return delete.IsAcknowledged;
        }

        public async Task<List<T>> FetchAll()
        {
            var filter = Builders<T>.Filter.Empty;
            var allData = await _mongoCollection.Find(filter).ToListAsync();
            return allData;
        }

    }
}
