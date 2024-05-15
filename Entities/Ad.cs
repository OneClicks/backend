using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace backend.Entities
{
    public class Ad
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("AdsetId")]
        public string AdsetId { get; set; }

        [BsonElement("AdsetName")]
        public string AdsetName { get; set; }

        [BsonElement("CreativeId")]
        public string CreativeId { get; set; }

        [BsonElement("AdAccountId")]
        public string AdAccountId { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; } 

        [BsonElement("AccessToken")]  
        public string AccessToken { get; set; } 
    }
}
