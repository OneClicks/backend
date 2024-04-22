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
using MongoDB.Bson.IO;
using backend.Services.API.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;


namespace backend.Services
{
    public class FacebookApiService : IFB
    {
        private readonly HttpClient _httpClient;
        private readonly FacebookApiOptions _facebookApiOptions;
        private readonly IGenericRepository<Campaigns> _campaignRepository;
        private readonly IGenericRepository<Adset> _adsetRepository;


        public FacebookApiService(HttpClient httpClient, IOptions<FacebookApiOptions> facebookApiOptions, IGenericRepository<Campaigns> camRepository, IGenericRepository<Adset> adsetRepository)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _facebookApiOptions = facebookApiOptions?.Value ?? throw new ArgumentNullException(nameof(facebookApiOptions));
            _campaignRepository = camRepository;
            _adsetRepository = adsetRepository;
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
                    Status = campaign.Status
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

        #region 2.get Targeting Spec City , Interest 

        public string CreateTargetingSpec(string countriesResponse, string interestsResponse)
        {
            // Assume we have extracted the country code from the countriesResponse
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
        }

        public async Task<ResponseVM<string>> GetCities(string query , string accessToken)
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
            return new ResponseVM<string> ("", "");
            
        }

        //public async Task<string> GetCities(string accessToken, string query)
        //{
        //    var url = $"https://graph.facebook.com/v19.0/search?type=adgeolocation&location_types=[\"country\"]&q={query}&access_token={accessToken}";

        //    using (var httpClient = new HttpClient())
        //    {
        //        var response = await _httpClient.GetAsync(url);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            throw new Exception($"Failed to get countries. Status code: {response.StatusCode}");
        //        }

        //        var content = await response.Content.ReadAsStringAsync();
        //        return content;
        //    }
        //}
        public class LocationData
        {
            public string Key { get; set; }
            public string CityName { get; set; }
            public string CountryName { get; set; }
        }
        public class AdTargetingCategory
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }


            // Add other properties as needed
        }

        //generic function for targeting classes : interests, behaviors, demographics, life_events, industries, income, family_statuses, user_device, user_os
        public async Task<ResponseVM<List<AdTargetingCategory>>>SearchAdTargetingCategories(string accessToken, string apiVersion, string targetType)
        {
            var url = $"https://graph.facebook.com/v{apiVersion}/search?type=adTargetingCategory&class={targetType}&access_token={accessToken}";

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

            return new ResponseVM<List<AdTargetingCategory>>("", "",adTargetingCategories);
        }

        public async Task<string> GetInterests(string accessToken, string query)
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
        public async Task<ResponseVM<string>> CreateAdSet(AdsetDto adset)
        {
            var targetingSpec = new Targeting
            {
                geo_locations = adset.Geolocations,
                interests = adset.Interests
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
                                       
                };
                await _adsetRepository.Create(adsetObj);
                var content = await response.Content.ReadAsStringAsync();
            }

      

            return new ResponseVM<string>("200", "Success");
        }
        #endregion

        #region 4. AD CREATIVE 
        public async Task<string> ProvideAdCreative(string accessToken, string query)
        {
            var imageHash = await UploadFile("D:\\TestPicture\\picture2.png", accessToken, adAccountId);
            var res = await CreateAdCreative(accessToken, adAccountId, "147986631741136", imageHash);
            return "";
        }
        public async Task<string> CreateAdCreative(string accessToken, string adAccountId, string pageId, string imageHash)
        {
            //var imageHash = "c4ac5393e1c91305d8de47dd166a4681";
            var url = $"https://graph.facebook.com/v19.0/act_{adAccountId}/adcreatives";
            var msg = "try it out";

            var requestBody = new
            {
                name = "Sample Creative SandBox",
                object_story_spec = new
                {
                    page_id = pageId,
                    link_data = new
                    {
                        link = $"https://facebook.com/{pageId}",
                        message = msg,
                        image_hash = imageHash
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
                access_token = accessToken
            };


            using (var httpClient = new HttpClient())
            {
                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

                //var formData = new MultipartFormDataContent();
                //formData.Add(new StringContent("Sample Creative "), "name");
                //formData.Add(new StringContent(jsonSpec), "object_story_spec");
                //formData.Add(new StringContent(degreesOfFreedomSpec), "degrees_of_freedom_spec");
                //formData.Add(new StringContent(accessToken), "access_token");

                var response = await httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create ad creative. Status code: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
        }

        public async Task<string> UploadFile(IFormFile imageFile, string accessToken, string adAccountId)
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

                return imageHash.ToString();
            }
        }
        public async Task<string> UploadFile(string imagePath, string accessToken, string adAccountId)
        {
            using (var httpClient = new HttpClient())
            {
                var formData = new MultipartFormDataContent();
                var fileStream = File.OpenRead(imagePath);
                formData.Add(new StreamContent(fileStream), "filename", Path.GetFileName(imagePath));
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
                var imageData = document.RootElement.GetProperty("images").GetProperty(Path.GetFileName(imagePath));
                var imageHash = imageData.GetProperty("hash").GetString();

                return imageHash.ToString();
            }
        }
        #endregion


        #region 5. Schedule Delivery of AD
        public async Task<ResponseVM<string>> ScheduleDelivery(string accessToken, string adAccountId, string adsetId, string adsetName, string creativeId)
        {
            var url = $"https://graph.facebook.com/v19.0/act_{adAccountId}/ads";
            var status = "PAUSED";

            using (var httpClient = new HttpClient())
            {
                var creative = new
                {
                    creative_id = creativeId
                };

                var jsonCreative = JsonSerializer.Serialize(creative);

                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent("My ad"), "name");
                formData.Add(new StringContent(adsetId), "adset_id");
                formData.Add(new StringContent(jsonCreative), "creative");
                formData.Add(new StringContent(status), "status");
                formData.Add(new StringContent(accessToken), "access_token");

                var response = await httpClient.PostAsync(url, formData);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create ad. Status code: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return new ResponseVM<string>("200", "Ad scheduled", responseContent); ;
            }
        }

        #endregion



        public async Task<ResponseVM<List<Campaigns>>> GetAllCampaigns()
        {
            var categories = await _campaignRepository.FetchAll();
            if (categories.Count == 0)
            {
                return new ResponseVM<List<Campaigns>>("404", "No Campaign Found!");
            }


            return new ResponseVM<List<Campaigns>>("200", "Successfully fetched all categories", categories);
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

       
    }
}