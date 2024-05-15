using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace backend.Entities
{
    public class AdCreative
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("CreativeId")]
        public string CreativeId { get; set; }

        [BsonElement("CreativeName")]
        public string CreativeName { get; set; }

        [BsonElement("Message")]
        public string Message { get; set; }

        [BsonElement("AdsetId")]
        public string AdsetId { get; set; }
        
        [BsonElement("FileName")]
        public string FileName { get; set; }

        [BsonElement("ImageHash")]
        public string ImageHash { get; set; }

        [BsonElement("AccessToken")]
        public string AccessToken { get; set; }

        [BsonElement("ImageFile")]
        public string ImageFile { get; set; }

        [BsonElement("Type")]
        public string Type { get; set; }
    }
}
