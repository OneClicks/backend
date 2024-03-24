using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace backend.Entities
{
    public class AdAccounts
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; }

        [BsonElement("AccountId")]
        public string AccountId { get; set; }

        [BsonElement("AdAccountId")]
        public string AdAccountId { get; set; }

        [BsonElement("Pages")]
        public List<string> Pages { get; set; }

        [BsonElement("Platform")]
        public string Platform { get; set; }
        [BsonElement("CampaignIds")]
        public List<string> CampaignIds { get; set; }
    }

}
