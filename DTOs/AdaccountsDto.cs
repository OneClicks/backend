using MongoDB.Bson.Serialization.Attributes;

namespace backend.DTOs
{
    public class AdaccountsDto
    {
        public string UserId { get; set; }

        public string AccountId { get; set; }

        public string AdAccountId { get; set; }

        public List<string> Pages { get; set; }

        public string Platform { get; set; }
        public string LongLiveToken { get; set; }
    }
    public class AdSetDto
    {
        public string AdsetName { get; set; }
        public string OptimizationGoal { get; set; }
        public string BillingEvent { get; set; }
        public decimal BidAmount { get; set; }
        public decimal DailyBudget { get; set; }
        public string CampaignId { get; set; }
        public List<string> GeoLocations { get; set; }
        public List<string> Interests { get; set; }
        public string StartTime { get; set; }
        public string Status { get; set; }
        public string AccessToken { get; set; }
    }

    public class Interest
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class Targeting
    {
        public GeoLocations Geo_Locations { get; set; }
        public List<Interest> Interests { get; set; }
    }

    public class GeoLocations
    {
        public List<string> Countries { get; set; }
    }
}
