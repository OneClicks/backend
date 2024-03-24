namespace backend.DTOs
{
    public class AdsetDto
    {
        public string AdsetName { get; set; }
        public string OptimizationGoal { get; set; }
        public string BillingEvent { get; set; }
        public int BidAmount { get; set; }
        public int DailyBudget { get; set; }
        public string CampaignId { get; set; }
        public List<string> Geolocations { get; set; }
        public string Interests { get; set; }
        public int StartTime { get; set; }
        public string Status { get; set; }
        public string AccessToken { get; set; }
    }
}
