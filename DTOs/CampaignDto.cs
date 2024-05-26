namespace backend.DTOs
{
    public class CampaignDto
    {
        public string CampaignName { get; set; }
        public string CampaignId { get; set; }
        public string Objective { get; set; }
        public List<string> SpecialAdCategories { get; set; }
        public string Status { get; set; }
        public string AdAccountId { get; set; }
        public string AccessToken { get; set; }
        public string Type { get; set; }

    }
}
