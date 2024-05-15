using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace backend.Entities
{
    public class RecentActivity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("Activity")]

        public string Activity { get; set; }
        [BsonElement("DateTime")]

        public string DateTime { get; set; }
    }
}
