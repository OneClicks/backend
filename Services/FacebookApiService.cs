using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.ViewModels;
using backend.Services.Interfaces;
using backend.Configurations;
using Microsoft.Extensions.Options;
using System.Text.Json;
using MongoDB.Bson;
using backend.DTOs;
using backend.Entities;
using backend.Repository.Interfaces;
using MongoDB.Driver;


namespace backend.Services
{
    public class FacebookApiService : IFB
    {
        private readonly HttpClient _httpClient;
        private readonly FacebookApiOptions _facebookApiOptions;
        private readonly IGenericRepository<Campaigns> _camRepository;


        public FacebookApiService(HttpClient httpClient, IOptions<FacebookApiOptions> facebookApiOptions, IGenericRepository<Campaigns> camRepository)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _facebookApiOptions = facebookApiOptions?.Value ?? throw new ArgumentNullException(nameof(facebookApiOptions));
            _camRepository = camRepository;
        }
        public async Task<ResponseVM<HttpResponseMessage>> CreateCampaignAsync(CampaignDto campaign)
        {
            try
            {
               // var adAccountId = "1295877481040276";
                //var accessToken= "EAAKbj1ZAaEcgBO6ZB9LAar9LdIphDG5TpxAePWZCUqFxWiVaBzpc01ruQoLKTBib8kUZA0ZCc6bzSYXZAJpGDaITIXXBZCzCc5Ww3HoalNcnZB12ACCmhSNwVBHHJusSUjcxZB6ZBESVes7mjZAQC2Cx5JRaVgXVJxZBPHQTM4rHrs2e2E48CA06gV0z63slsJ0ybmlbOsn2qWLiqdIgtlZCWjwZDZD";
                //var url = $"https://graph.facebook.com/v19.0/act_{adAccountId}/campaigns"; 
                //var formData = new MultipartFormDataContent(); 
                //formData.Add(new StringContent("My campaign"), "name"); 
                //formData.Add(new StringContent("OUTCOME_TRAFFIC"), "objective"); 
                //formData.Add(new StringContent("PAUSED"), "status");
                //formData.Add(new StringContent("[]"), "special_ad_categories"); 
                //formData.Add(new StringContent(accessToken), "access_token");

                var url = $"https://graph.facebook.com/v19.0/act_{campaign.Ad_accountId}/campaigns";
                //campaign.AccessToken = "EAAKbj1ZAaEcgBO6OQLPWFZAa3ttZAI0UewZBxuZAvcLlBkbZA0HjQhOvjKcTVmBiygrZAKqxdrYMIc7gw9gZC5J5YuV5Vmft4OunJhS0fYIuFJceV8cZBLCYIFwwOdboHeBI4uaMWVF1mboWluLCV9Pp6mXae1evUcV9F1XTuaCQuod9VebJTciiQhyLsFQZDZD";
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(campaign.CampaignName), "name");
                formData.Add(new StringContent(campaign.Objective), "objective");
                formData.Add(new StringContent(campaign.Status), "status");
                formData.Add(new StringContent("[]"), "special_ad_categories");
                formData.Add(new StringContent(campaign.AccessToken), "access_token");


                var response = await _httpClient.PostAsync(url, formData);
                var responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(responseContent);

                var jsonResponse = document.RootElement.GetProperty("id").GetString();
                var newCampaign = new Campaigns
                {
                    CampaignId = jsonResponse,
                    CampaignName = campaign.CampaignName,
                    Objective = campaign.Objective,
                    AccessToken = campaign.AccessToken,
                    AdAccountId = campaign.Ad_accountId,
                    SpecialAdCategories = campaign.SpecialAdCategories,
                    Status = campaign.Status
                };
                await _camRepository.Create(newCampaign);
                if (response.IsSuccessStatusCode)
                {
                    return new ResponseVM<HttpResponseMessage>("200", "Successfully Created campaign", response);

                }
                return new ResponseVM<HttpResponseMessage>("400", "Cannot Create campaign");

            }
            catch (Exception ex)
            {
                // Log exception here
                Console.WriteLine(ex.ToString());
                return new ResponseVM<HttpResponseMessage>("200", "Cannot Created campaign");

            }
        }

        public async Task<ResponseVM<List<Campaigns>>> GetAllCampaigns()
        {
            var categories = await _camRepository.FetchAll();
            if (categories.Count == 0)
            {
                return new ResponseVM<List<Campaigns>>("404", "No Campaign Found!");
            }

           
            return new ResponseVM<List<Campaigns>>("200", "Successfully fetched all categories", categories);
        }

        public async Task<string> SearchAdInterest(string accessToken, string query, string apiVersion)
        {
            var url = $"https://graph.facebook.com/v19.0/search";

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("adinterest"), "type");
            formData.Add(new StringContent(query), "q");
            formData.Add(new StringContent(accessToken), "access_token");

            var response = await _httpClient.PostAsync(url, formData);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to search for ad interest. Status code: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        public async Task<string> GetCountries(string accessToken, string query)
        {
            var url = $"https://graph.facebook.com/v19.0/search?type=adgeolocation&location_types=[\"country\"]&q={query}&access_token={accessToken}";

            using (var httpClient = new HttpClient())
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to get countries. Status code: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
        }
        public async Task<string> GetInterests(string accessToken,string query)
        {
            var url = $"https://graph.facebook.com/v19.0/search?type=adinterest&q={query}&access_token={accessToken}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get interests. Status code: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
          

            return content;
        }

        public string CreateTargetingSpec(string countriesResponse, string interestsResponse)
        {
            // Assume we have extracted the country code from the countriesResponse
            var countries = new List<string> { "US" };

            // Parse the interestsResponse to get the interest ID
            var interestsData = JsonSerializer.Deserialize<dynamic>(interestsResponse);
            var interests = new List<Interest> { new Interest { Id = interestsData.id, Name = interestsData.name } };

            var targetingSpec = new Targeting
            {
                Geo_Locations = new GeoLocations { Countries = countries },
                Interests = interests
            };

            return JsonSerializer.Serialize(targetingSpec);
        }

        public async Task<ResponseVM<string>> CreateAdSet()
        {
            var accessToken = "EAAKbj1ZAaEcgBO3yYvqRazffDMELK32LyMscrWSjkyzuwTLjVv8ZBhjC1NzPmvUgEteKyOi0yZA3jmTHEFZBPxvdFY3UJZBxsS3nkCB0dlxQvZARrJntOys7txbFgx7ajdd2eMZCMgiv1ZCW2E8ZCEZChckmSI0alBjn074ve8ioD4uDai3IHw9SUbDEy1";
            var adAccountId = "1295877481040276";
            var campaignId = "120206143859950113";
            var interestsResponse = await GetInterests(accessToken, "movie");
            using JsonDocument document = JsonDocument.Parse(interestsResponse);
            var interest = document.RootElement.GetProperty("data").EnumerateArray().First();

            // Assume we have extracted the country code from the countriesResponse
            var countries = new List<string> { "BA" };

            var interests = new List<Interest> { new Interest { Id = interest.GetProperty("id").GetInt64(), Name = interest.GetProperty("name").GetString() } };

            var targetingSpec = new Targeting
            {
                Geo_Locations = new GeoLocations { Countries = countries },
                Interests = interests
            };

            var targetingJson = JsonSerializer.Serialize(targetingSpec);
            // Create the ad set
            var url = $"https://graph.facebook.com/v19.0/act_{adAccountId}/adsets";

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("My Ad Set"), "name");
            formData.Add(new StringContent("REACH"), "optimization_goal");
            formData.Add(new StringContent("IMPRESSIONS"), "billing_event");
            formData.Add(new StringContent("2"), "bid_amount");
            formData.Add(new StringContent("1000"), "daily_budget");
            formData.Add(new StringContent(campaignId), "campaign_id");
            formData.Add(new StringContent(targetingJson), "targeting");
            formData.Add(new StringContent("2020-10-06T04:45:17+0000"), "start_time");
            formData.Add(new StringContent("PAUSED"), "status");
            formData.Add(new StringContent(accessToken), "access_token");

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(url, formData);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create ad set. Status code: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                return new ResponseVM<string>("200", " Created adset", content);

            }
        }

        public async Task<string> GetLongLivedToken(string accessToken)
        {
            var url = $"https://graph.facebook.com/{_facebookApiOptions.GraphApiVersion}/oauth/access_token" +
                        $"?grant_type=fb_exchange_token" +
                        $"&client_id={_facebookApiOptions.AppId}" +
                        $"&client_secret={_facebookApiOptions.AppSecret}" +
                        $"&fb_exchange_token={accessToken}";


            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(content);

                return document.RootElement.GetProperty("access_token").GetString();
            }
            else
            {
                throw new Exception($"Failed to exchange access token. Status code: {response.StatusCode}");
            }
        }

        public async Task<ResponseVM<AdaccountsDto>> GetAdAccountsData(string accessToken)
        {
            var LongLivedToken = await GetLongLivedToken(accessToken);

            var url = $"https://graph.facebook.com/v18.0/me" +
                      $"?fields=id,name,accounts,adaccounts" +
                      $"&access_token={accessToken}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(content);

                var adAccounts = document.RootElement;
                var pageIds = new List<string>();
                foreach (var account in adAccounts.GetProperty("accounts").GetProperty("data").EnumerateArray())
                {
                    pageIds.Add(account.GetProperty("id").GetString());
                }
                var data = new AdaccountsDto
                {
                    AccountId = adAccounts.GetProperty("id").GetString(),
                    AdAccountId = adAccounts.GetProperty("adaccounts").GetProperty("data").EnumerateArray().ElementAt(0).GetProperty("account_id").GetString(),
                    Pages = pageIds,
                    Platform ="Facebook",
                    UserId=adAccounts.GetProperty("id").GetString(), 
                    LongLiveToken = LongLivedToken


            };
                return new ResponseVM<AdaccountsDto>("200","success",data);
            }
            else
            {
                throw new Exception($"Failed to fetch user data. Status code: {response.StatusCode}");
            }
        }
        //get pageid
        //curl req to fb 
        //return response
    }
}