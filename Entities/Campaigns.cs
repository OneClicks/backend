using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace backend.Entities
{
    public class Campaigns
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("CampaignId")]
        public string CampaignId { get; set; }

        [BsonElement("AdAccountId")]
        public string AdAccountId { get; set; }

        [BsonElement("Name")]
        public string CampaignName { get; set; }  // From original class

        [BsonElement("Objective")]
        public string Objective { get; set; }  // From original class

        [BsonElement("SpecialAdCategories")]
        public List<string> SpecialAdCategories { get; set; }  // From original class

        [BsonElement("Status")]
        public string Status { get; set; }  // From original class

        [BsonElement("AccessToken")]  // Consider if needed for your specific use case
        public string AccessToken { get; set; }  // Optional from original class

        [BsonElement("Type")]  
        public string Type { get; set; } 
    }

}
