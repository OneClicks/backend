using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.ViewModels;
using backend.ServiceFiles.Interfaces;
using backend.Configurations;
using Microsoft.Extensions.Options;
using System.Text.Json;
using MongoDB.Bson;
using backend.DTOs;
using backend.Entities;
using backend.Repository.Interfaces;
using MongoDB.Driver;
using MongoDB.Bson.IO;
using backend.ServiceFiles.API.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms;
using System.Xml.Linq;

namespace backend.ServiceFiles
{
    public class FacebookApiService : IFB
    {
        private readonly HttpClient _httpClient;
        private readonly FacebookApiOptions _facebookApiOptions;
        private readonly IGenericRepository<Campaigns> _campaignRepository;
        private readonly IGenericRepository<Adset> _adsetRepository;
        private readonly IGenericRepository<AdCreative> _adcreativeRepository;
        private readonly IGenericRepository<Ad> _adRepository;

        public FacebookApiService(HttpClient httpClient, IOptions<FacebookApiOptions> facebookApiOptions, 
            IGenericRepository<Campaigns> camRepository, IGenericRepository<Adset> adsetRepository,
            IGenericRepository<AdCreative> adcreativeRepository, IGenericRepository<Ad> adRepository)
            {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _facebookApiOptions = facebookApiOptions?.Value ?? throw new ArgumentNullException(nameof(facebookApiOptions));
            _campaignRepository = camRepository;
            _adsetRepository = adsetRepository;
            _adcreativeRepository = adcreativeRepository;
            _adRepository = adRepository;
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
                    Platform = "Facebook",
                    UserId = adAccounts.GetProperty("id").GetString(),
                    LongLiveToken = LongLivedToken


                };
                return new ResponseVM<AdaccountsDto>("200", "success", data);
            }
            else
            {
                throw new Exception($"Failed to fetch user data. Status code: {response.StatusCode}");
            }
        }

        #region 1. create campaign
        public async Task<ResponseVM<object>> GetCampaignData(string accessToken, string adAccountId)
        {
            using (var httpClient = new HttpClient())
            {
                var url = $"https://graph.facebook.com/v18.0/act_{adAccountId}?fields=campaigns{{name,id,objective,status}}&access_token={accessToken}";

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch campaign data. Status code: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(responseContent);

                var campaigns = document.RootElement.GetProperty("campaigns").GetProperty("data");

                var campaignData = new List<object>();
                foreach (var campaign in campaigns.EnumerateArray())
                {
                    campaignData.Add(new
                    {
                        CampaignName = campaign.GetProperty("name").GetString(),
                        CampaignId = campaign.GetProperty("id").GetString(),
                        Objective = campaign.GetProperty("objective").GetString(),
                        Status = campaign.GetProperty("status").GetString(),
                        Type = "Facebook"
                    });
                }

                return new ResponseVM<object>("200", "Campaign data fetched", campaignData);
            }
        }

        public async Task<ResponseVM<HttpResponseMessage>> CreateCampaign(CampaignDto campaign)
        {
            try
            {
                var url = $"https://graph.facebook.com/v19.0/act_{campaign.AdAccountId}/campaigns";
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(campaign.CampaignName), "name");
                formData.Add(new StringContent(campaign.Objective), "objective");
                formData.Add(new StringContent(campaign.Status), "status");
                formData.Add(new StringContent("[]"), "special_ad_categories"); // TODO - have to check the param for fb (dont know this field requirements)
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
                    AdAccountId = campaign.AdAccountId,
                    SpecialAdCategories = campaign.SpecialAdCategories,
                    Status = campaign.Status,
                    Type = campaign.Type
                };
                if (response.IsSuccessStatusCode)
                {
                    await _campaignRepository.Create(newCampaign);
                    return new ResponseVM<HttpResponseMessage>("200", "Successfully Created campaign", response);

                }
                return new ResponseVM<HttpResponseMessage>("400", "Cannot Create campaign",response);

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
            var categories = await _campaignRepository.FetchAll();
            if (categories.Count == 0)
            {
                return new ResponseVM<List<Campaigns>>("404", "No Campaign Found!");
            }
            return new ResponseVM<List<Campaigns>>("200", "Successfully fetched all categories", categories);
        }
        #endregion

        #region 2.get Targeting Spec City , Interest 
/*        public string CreateTargetingSpec(string countriesResponse, string interestsResponse)
        {
            // Assume we have extracted the country code from the +countriesResponse
            var countries = new List<string> { "US" };

            // Parse the interestsResponse to get the interest ID
            var interestsData = JsonSerializer.Deserialize<dynamic>(interestsResponse);
            var interests = new List<Interest> { new Interest { id = interestsData.id, name = interestsData.name } };

            var targetingSpec = new Targeting
            {
                geo_locations = new GeoLocations { countries = countries },
                interests = interests
            };

            return JsonSerializer.Serialize(targetingSpec);
        }*/

        public async Task<ResponseVM<List<LocationData>>> GetCities(string accessToken, string query )
        {
            var url = $"https://graph.facebook.com/v19.0/search";
            var queryParams = new Dictionary<string, string>
            {
                { "location_types", "[\"city\"]" },
                { "type", "adgeolocation" },
                { "q", query },
                { "access_token", accessToken }
            };

            url = QueryHelpers.AddQueryString(url, queryParams);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to search location. Status code: {response.StatusCode}");
            }
            var json = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(json);
            var locations = new List<LocationData>();

            foreach (var data in document.RootElement.GetProperty("data").EnumerateArray())
            {
                locations.Add(new LocationData
                {

                    CountryName = data.GetProperty("country_name").GetString(),

                    Key = data.GetProperty("key").GetString(),
                    CityName = data.GetProperty("name").GetString()
                });
            }
            return new ResponseVM<List<LocationData>> ("200", "Successfully fetched cities", locations);
            
        }

        //generic function for targeting classes : interests, behaviors, demographics, life_events, industries, income, family_statuses, user_device, user_os
        //get all (targetType = interests etc)
        public async Task<ResponseVM<List<AdTargetingCategory>>>SearchAdTargetingCategories(string accessToken, string targetType)
        {
            var url = $"https://graph.facebook.com/v19.0/search?type=adTargetingCategory&class={targetType}&access_token={accessToken}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to search ad targeting categories. Status code: {response.StatusCode}");
            }
                        
            var json = await response.Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(json);
            var adTargetingCategories = new List<AdTargetingCategory>();

            foreach (var data in document.RootElement.GetProperty("data").EnumerateArray())
            {
                adTargetingCategories.Add(new AdTargetingCategory
                {

                    Type = data.GetProperty("type").GetString(),
                    Name = data.GetProperty("name").GetString(),
                    Id = data.GetProperty("id").GetString(), 
                    Description = data.GetProperty("description").GetString()
                });
            }

            return new ResponseVM<List<AdTargetingCategory>>("200", "Successfully fetch targeting categories",adTargetingCategories);
        }

        public async Task<ResponseVM<List<Interest>>> GetInterests(string accessToken, string query)
        {
            var url = $"https://graph.facebook.com/v19.0/search?type=adinterest&q={query}&access_token={accessToken}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get interests. Status code: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
           var interestList = new List<Interest>();
            using JsonDocument document = JsonDocument.Parse(content);

            var interest = document.RootElement;
            foreach (var temp in interest.GetProperty("data").EnumerateArray())
            {
                interestList.Add(new Interest
                {
                    id = long.Parse(temp.GetProperty("id").GetString()),
                    name = temp.GetProperty("name").GetString()

                });
            }

            if (content == null)
            {
                return new ResponseVM< List < Interest >> ("400", "Unable to fetch interests");
            }

            return new ResponseVM<List<Interest>>("200", "Successfully fetched interests", interestList);
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
        #endregion

        #region 3.AD Set 
        public async Task<ResponseVM<HttpResponseMessage>> CreateAdSet(AdsetDto adset)
        {
            var targetingSpec = new Targeting
            {
                geo_locations = adset.Geolocations,
                interests = adset.Interests, 
                industries = adset.Industries
            };

        var targetingJson = JsonSerializer.Serialize(targetingSpec);
            // Create the ad set
            var url = $"https://graph.facebook.com/v19.0/act_{adset.AdAccountId}/adsets";

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(adset.AdsetName), "name");
            formData.Add(new StringContent(adset.OptimizationGoal), "optimization_goal");
            formData.Add(new StringContent(adset.BillingEvent), "billing_event");
            formData.Add(new StringContent(adset.BidAmount.ToString()), "bid_amount");
            formData.Add(new StringContent(adset.DailyBudget.ToString()), "daily_budget");
            formData.Add(new StringContent(adset.CampaignId), "campaign_id");
            formData.Add(new StringContent(targetingJson), "targeting");
            formData.Add(new StringContent(adset.StartTime), "start_time");
            formData.Add(new StringContent(adset.Status), "status");
            formData.Add(new StringContent(adset.AccessToken), "access_token");

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(url, formData);
                var responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(responseContent);

                var jsonResponse = document.RootElement.GetProperty("id").GetString();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create ad set. Status code: {response.StatusCode}");
                }
                var adsetObj = new Adset
                {
                    DailyBudget = adset.DailyBudget.ToString(),
                    AccessToken = adset.AccessToken,
                    AdsetId = jsonResponse,
                    AdsetName = adset.AdsetName,
                    BidAmount = adset.BidAmount.ToString(),
                    BillingEvent = adset.BillingEvent,
                    CampaignId = adset.CampaignId,
                    Geolocations = adset.Geolocations,
                    Interests = adset.Interests,
                    OptimizationGoal = adset.OptimizationGoal,
                    StartTime = adset.StartTime,
                    Status = adset.Status,
                    Type = adset.Type
                };

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    await _adsetRepository.Create(adsetObj);
                    return new ResponseVM<HttpResponseMessage>("200", "Successfully created adset", response);

                }
                return new ResponseVM<HttpResponseMessage>("400", "Cannot create adset", response);

            }   
        }

        public async Task<ResponseVM<object>> GetAdSetData(string accessToken, string adAccountId)
        {
            using (var httpClient = new HttpClient())
            {
                var url = $"https://graph.facebook.com/v18.0/act_{adAccountId}?fields=adsets{{id, account_id,campaign_id, daily_budget,targeting,optimization_goal,status,start_time,name}}&access_token={accessToken}";

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch ad set data. Status code: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(responseContent);

                var adSets = document.RootElement.GetProperty("adsets").GetProperty("data");

                var adSetData = new List<object>();
                foreach (var adSet in adSets.EnumerateArray())
                {
                    adSetData.Add(new
                    {
                        AdAccountId = adSet.GetProperty("account_id").GetString(),
                        CampaignId = adSet.GetProperty("campaign_id").GetString(),
                        AdsetId = adSet.GetProperty("id").GetString(),
                        DailyBudget = int.Parse(adSet.GetProperty("daily_budget").GetString()),
                        OptimizationGoal = adSet.GetProperty("optimization_goal").GetString(),
                        Status = adSet.GetProperty("status").GetString(),
                        StartTime = adSet.GetProperty("start_time").GetString(),
                        AdsetName = adSet.GetProperty("name").GetString(),
                        Type = "Facebook"
                    });
                }

                return new ResponseVM<object>("200", "Ad set data fetched", adSetData);
            }
        }

        #endregion

        public async Task<ResponseVM<List<Adset>>> GetAllAdsets()
        {
            var adsets = await _adsetRepository.FetchAll();
            if (adsets.Count == 0)
            {
                return new ResponseVM<List<Adset>>("404", "No Adset Found!");
            }
            return new ResponseVM<List<Adset>>("200", "Successfully fetched all adsets", adsets);
        }

        #region 4. AD CREATIVE 
 /*       public async Task<ResponseVM<AdCreative>> ProvideAdCreative(AdCreativeDto creative)
        {
            creative.ImageHash = await UploadFile(creative.ImageFile, creative.AccessToken, creative.AdAccountId);
            var res = await CreateAdCreative(creative);

            return new ResponseVM<AdCreative>("200", "Successfully created adcreative", res.ResponseData);
        }*/
        public async Task<ResponseVM<AdCreative>> CreateAdCreative(AdCreativeDto creative)
        {
            var url = $"https://graph.facebook.com/v19.0/act_{creative.AdAccountId}/adcreatives";
            var msg = "try it out";

            var requestBody = new
            {
                name = creative.CreativeName,
                object_story_spec = new
                {
                    page_id = creative.PageId,
                    link_data = new
                    {
                        link = $"https://facebook.com/{creative.PageId}",
                        message = msg,
                        image_hash = creative.ImageHash
                    }
                },
                degrees_of_freedom_spec = new
                {
                    creative_features_spec = new
                    {
                        standard_enhancements = new
                        {
                            enroll_status = "OPT_IN"
                        }
                    }
                },
                access_token = creative.AccessToken
            };


            using (var httpClient = new HttpClient())
            {
                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create ad creative. Status code: {response.StatusCode}");
                }
                var responseContent = await response.Content.ReadAsStringAsync();

                var adcreativeObj = new AdCreative
                { 
                    CreativeId = responseContent,
                    CreativeName = creative.CreativeName,
                    AdsetId = creative.AdsetId,
                    AccessToken = creative.AccessToken,
                    Message = creative.Message,
                    FileName = creative.FileName,
                    ImageHash = creative.ImageHash,
                    Type = creative.Type,
                };
                var responseCreative = await _adcreativeRepository.Create(adcreativeObj);
                
                return new ResponseVM<AdCreative>("200", "Successfully created adcreative", responseCreative);                               
            }
        }

        public async Task<ResponseVM<string>> UploadFile(string accessToken, IFormFile imageFile, string adAccountId)
        //(IFormFile imageFile, string accessToken, string adAccountId)
        {
            using (var httpClient = new HttpClient())
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new StreamContent(imageFile.OpenReadStream()), "filename", imageFile.FileName);
                formData.Add(new StringContent(accessToken), "access_token");

                var url = $"https://graph.facebook.com/v19.0/act_{adAccountId}/adimages";

                var response = await httpClient.PostAsync(url, formData);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to upload file. Status code: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                using JsonDocument document = JsonDocument.Parse(responseContent);
                var imageData = document.RootElement.GetProperty("images").GetProperty(imageFile.FileName);
                var imageHash = imageData.GetProperty("hash").GetString();
                if (imageHash != null)
                {
                    return new ResponseVM<string>("200", "Successfully created image hash", imageHash.ToString());
                }

                return new ResponseVM<string>("404", "Couldnt create image hash");
            }
        }

        public async Task<ResponseVM<List<AdCreative>>> GetAllAdcreatives()
        {
            var adcreative = await _adcreativeRepository.FetchAll();
            if (adcreative.Count == 0)
            {
                return new ResponseVM<List<AdCreative>>("404", "No Adcreatives Found!");
            }
            return new ResponseVM<List<AdCreative>>("200", "Successfully fetched all adcreatives", adcreative);
        }
        #endregion

        #region 5. Schedule Delivery of AD
        public async Task<ResponseVM<object>> GetPayloadForAd(string accessToken, string adAccountId)
        {

            using (var httpClient = new HttpClient())
            {
                var url = $"https://graph.facebook.com/v18.0/act_{adAccountId}?fields=adcreatives{{name}},campaigns{{name}},adsets{{name, campaign_id}}&access_token={accessToken}";

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch user data. Status code: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(responseContent);
                var adCreativeData = new List<object>();
                var adCreativeArray = document.RootElement.GetProperty("adcreatives").GetProperty("data");
                foreach (var item in adCreativeArray.EnumerateArray())
                {
                    adCreativeData.Add(new
                    {
                        Id = item.GetProperty("id").GetString(),
                        Name = item.GetProperty("name").GetString()
                    });
                }

                var campaignData = new List<object>();
                var campaignArray = document.RootElement.GetProperty("campaigns").GetProperty("data");
                foreach (var item in campaignArray.EnumerateArray())
                {
                    campaignData.Add(new
                    {
                        Id = item.GetProperty("id").GetString(),
                        Name = item.GetProperty("name").GetString(),
                        CampaignId = item.GetProperty("campaign_id").GetString()
                    });
                }

                var adSetData = new List<object>();
                var adSetArray = document.RootElement.GetProperty("adsets").GetProperty("data");
                foreach (var item in adSetArray.EnumerateArray())
                {
                    adSetData.Add(new
                    {
                        Id = item.GetProperty("id").GetString(),
                        Name = item.GetProperty("name").GetString()
                    });
                }

                var responseDto = new
                {
                    campaignData = campaignData,
                    adSetData = adSetData,
                    adCreativeData = adCreativeData
                };

                return new ResponseVM<object>("200", "Ad scheduled", responseDto);

            }
        }

        public async Task<ResponseVM<string>> ScheduleDelivery(AdDto ad)
        {
            var url = $"https://graph.facebook.com/v19.0/act_{ad.AdAccountId}/ads";
            var status = "PAUSED";

            using (var httpClient = new HttpClient())
            {
                var creative = new
                {
                    creative_id = ad.CreativeId
                };

                var jsonCreative = JsonSerializer.Serialize(creative);

                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent("My ad"), "name");
                formData.Add(new StringContent(ad.AdsetId), "adset_id");
                formData.Add(new StringContent(jsonCreative), "creative");
                formData.Add(new StringContent(status), "status");
                formData.Add(new StringContent(ad.AccessToken), "access_token");

                var response = await httpClient.PostAsync(url, formData);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create ad. Status code: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return new ResponseVM<string>("200", "Ad scheduled", responseContent); 

                var adDto = new Ad
                {
                    AdAccountId = ad.AdAccountId,
                    AdsetName = ad.AdsetId,
                    Status = ad.Status,
                    AdsetId = ad.AdsetId,
                    AccessToken = ad.AccessToken,
                };
                var responseAd = await _adRepository.Create(adDto);

                return new ResponseVM<string>("200", "Successfully created adcreative", responseAd.ToString());
            }
        }
        #endregion

        public async Task<ResponseVM<string>> GetCampaignInsights(string campaignId, string accessToken)
        {
            string datePreset = "last_7d";
            string fields = "impressions";

            // Construct the URL
            string url = $"https://graph.facebook.com/{_facebookApiOptions.GraphApiVersion}/{campaignId}/insights";
            url += $"?fields={fields}&date_preset={datePreset}&access_token={accessToken}";

            using (var httpClient = new HttpClient())
            {
               var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to get insights. Status code: {response.StatusCode}");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(responseContent);
                var responseData = document.RootElement.GetProperty("report_run_id").GetString();

                return new ResponseVM<string> ("200","Interests data", responseData);
            }
        }

        public async Task<ResponseVM<string>> GetAdAccountInsights(string adAccountId, string accessToken)
        {
            string datePreset = "last_7d";
            string fields = "impressions";

            // Construct the URL
            string url = $"https://graph.facebook.com/{_facebookApiOptions.GraphApiVersion}/act_{adAccountId}/insights";
            url += $"?fields={fields}&date_preset={datePreset}&access_token={accessToken}";

            using (var httpClient = new HttpClient())
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to get insights. Status code: {response.StatusCode}");
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(responseContent);

                return new ResponseVM<string>("200", "Interests data", responseContent);
            }
        }
    }
}