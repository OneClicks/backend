using backend.Entities;
using backend.Repository.Interfaces;
using backend.ServiceFiles.Interfaces;
using backend.ViewModels;
using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds;
using System.Text.Json;
using backend.Configurations;
using Google.Ads.GoogleAds.V16.Services;
using Google.Ads.GoogleAds.V16.Errors;

namespace backend.ServiceFiles
{
    public class InsightsService 
    {
        private readonly IFB _facebookService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IGenericRepository<RecentActivity> _recentRepository;

        public InsightsService(IFB facebookService, IGenericRepository<RecentActivity> recentRepository, IGoogleApiService googleApiService)
        {
            _facebookService = facebookService;
            _recentRepository = recentRepository;
            _googleApiService = googleApiService;
        }
        public async Task<ResponseVM<object>> GetBudgetAmountFacebook(string accessToken, string adAccountId)
        {
            using (var httpClient = new HttpClient())
            {
                var url = $"https://graph.facebook.com/v18.0/act_{adAccountId}?fields=adsets{{id, account_id,campaign_id, daily_budget,status,start_time,name}}&access_token={accessToken}";

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch ad set data. Status code: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(responseContent);

                var adSets = document.RootElement.GetProperty("adsets").GetProperty("data");
                long totalAmount = 0;

                var adSetData = new List<object>();
                foreach (var adSet in adSets.EnumerateArray())
                {
                    adSetData.Add(new
                    {
                        CampaignId = adSet.GetProperty("campaign_id").GetString(),
                       
                        DailyBudget = int.Parse(adSet.GetProperty("daily_budget").GetString()),
                        Status = adSet.GetProperty("status").GetString(),
                        StartTime = adSet.GetProperty("start_time").GetString(),
                        Type = "Facebook"
                    });
                    totalAmount += int.Parse(adSet.GetProperty("daily_budget").GetString());
                }
              

                return new ResponseVM<object>("200", "Ads set data fetched", $"{totalAmount} RS");
            }
        }
        public async Task<ResponseVM<long>> GetBudgetAmountGoogle(string refreshToken, string customer, string manager)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = refreshToken,
                LoginCustomerId = manager.ToString()
            };

            GoogleAdsClient client = new GoogleAdsClient(config);
            var googleAdsService = client.GetService(Services.V16.GoogleAdsService);

            string query = @"SELECT
                                campaign.id,
                                campaign.campaign_budget,
                                FROM campaign
                                ORDER BY campaign.id";

            try
            {
                long totalAmount = 0;
                List<object> campaignDetails = new List<object>();
                googleAdsService.SearchStream(customer.ToString(), query,
                    async delegate (SearchGoogleAdsStreamResponse resp)
                    {
                        foreach (GoogleAdsRow googleAdsRow in resp.Results)
                        {

                            var str= await _googleApiService.GetCampaignBudget(customer, manager, googleAdsRow.Campaign.CampaignBudget, refreshToken);
                            totalAmount = totalAmount + long.Parse(str);
                        }
                    }
                );
                return new ResponseVM<long>("200", "Successfully fetched google campaigns", totalAmount);
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }
        }

        public async Task<ResponseVM<object>> GetRecentActivity()
        {

        var recent = await _recentRepository.FetchAll();
        if (recent.Count == 0)
        {
            return new ResponseVM<object>("404", "NO recent activities");

        }
            

        return new ResponseVM<object>("200", "Recent activities fetched", recent.TakeLast(6).ToList());
            
        }
        public async Task<ResponseVM<object>> AddRecentActivity()
        {

            var temp = new RecentActivity
            {
                Activity = "New Campaign Added: Facebook",
                DateTime = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt")
            };
            await _recentRepository.Create(temp);

            return new ResponseVM<object>("200", "created", temp);

        }
    }
}
