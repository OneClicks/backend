using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace backend.Entities
{
    public class Adset
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("AdsetId")]
        public string AdsetId { get; set; }

        [BsonElement("AdsetName")]
        public string AdsetName { get; set; }

        [BsonElement("OptimizationGoal")]
        public string OptimizationGoal { get; set; }  // From original class

        [BsonElement("BillingEvent")]
        public string BillingEvent { get; set; }

        [BsonElement("BidAmount")]
        public string BidAmount { get; set; } // From original class

        [BsonElement("DailyBudget")]
        public string DailyBudget { get; set; }
        [BsonElement("CampaignId")]
        public string CampaignId { get; set; }

        [BsonElement("Geolocations")]
        public List<string> Geolocations { get; set; }  // From original class

        [BsonElement("Interests")]
        public string Interests { get; set; }

        [BsonElement("StartTime")]
        public string StartTime { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; }
        
        [BsonElement("AccessToken")]  // Consider if needed for your specific use case
        public string AccessToken { get; set; }  // Optional from original class

    }
}
