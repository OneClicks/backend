namespace backend.DTOs
{
    public class AdsetDto
    {
        public string AdAccountId { get; set; }
        public string AdsetName { get; set; }
        public string OptimizationGoal { get; set; }
        public string BillingEvent { get; set; }
        public int BidAmount { get; set; }
        public int DailyBudget { get; set; }
        public string CampaignId { get; set; }
        public GeoLocations Geolocations { get; set; }
        public List<Interest> Interests { get; set; }
        public string StartTime { get; set; }
        public string Status { get; set; }
        public string AccessToken { get; set; }
    }
}
