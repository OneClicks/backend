namespace backend.DTOs.GoogleDtos
{
    public class AdGroupDto
    {
        public long CustomerId { get; set; }
        public long CampaignId { get; set; }
        public string CampaignName { get; set; }
        public string AdGroupName { get; set; }
        public string AdGroupBidAmount { get; set; }
        public string AdGroupStatus { get; set; }
        public string RefreshToken { get; set; }
        public long ManagerId { get; set; }
        public string Type { get; set; }
        public SearchAdDto SearchAds { get; set; }
        public KeywordsDto Keywords { get; set; }
        public GeoTargetingDto GeoTargeting { get; set; }
    }
}
