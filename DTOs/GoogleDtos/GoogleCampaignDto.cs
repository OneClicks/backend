namespace backend.DTOs.GoogleDtos
{
    public class GoogleCampaignDto
    {
        public string CampaignName { get; set; }
        public string CampaignId { get; set; }
        public string AdvertisingChannelType { get; set; }
        public bool TargetGoogleSearch { get; set; }
        public bool TargetSearchNetwork { get; set; }
        public string BudgetAmount { get; set; }
        public string BudgetName { get; set; }
        public string BudgetDeliveryMethod { get; set; }


        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Status { get; set; }
        public long CustomerId { get; set; }
        public string RefreshToken { get; set; }
        public string Type { get; set; }
    }
}
