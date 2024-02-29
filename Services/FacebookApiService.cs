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


namespace backend.Services
{
    public class FacebookApiService : IFB
    {
        private readonly HttpClient _httpClient;
        private readonly FacebookApiOptions _facebookApiOptions;

        public FacebookApiService(HttpClient httpClient, IOptions<FacebookApiOptions> facebookApiOptions)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _facebookApiOptions = facebookApiOptions?.Value ?? throw new ArgumentNullException(nameof(facebookApiOptions));
        }
        public async Task<ResponseVM<HttpResponseMessage>> CreateCampaignAsync(CampaignDto campaign)
        {
            try
            {
                var adAccountId = "1295877481040276";
                var accessToken= "EAAKbj1ZAaEcgBO6ZB9LAar9LdIphDG5TpxAePWZCUqFxWiVaBzpc01ruQoLKTBib8kUZA0ZCc6bzSYXZAJpGDaITIXXBZCzCc5Ww3HoalNcnZB12ACCmhSNwVBHHJusSUjcxZB6ZBESVes7mjZAQC2Cx5JRaVgXVJxZBPHQTM4rHrs2e2E48CA06gV0z63slsJ0ybmlbOsn2qWLiqdIgtlZCWjwZDZD";
                var url = $"https://graph.facebook.com/v19.0/act_{adAccountId}/campaigns"; 
                var formData = new MultipartFormDataContent(); 
                formData.Add(new StringContent("My campaign"), "name"); 
                formData.Add(new StringContent("OUTCOME_TRAFFIC"), "objective"); 
                formData.Add(new StringContent("PAUSED"), "status");
                formData.Add(new StringContent("[]"), "special_ad_categories"); 
                formData.Add(new StringContent(accessToken), "access_token");

              //  var url = $"https://graph.facebook.com/v19.0/act_{campaign.Ad_accountId}/campaigns";
             //   campaign.AccessToken = "EAAKbj1ZAaEcgBO6OQLPWFZAa3ttZAI0UewZBxuZAvcLlBkbZA0HjQhOvjKcTVmBiygrZAKqxdrYMIc7gw9gZC5J5YuV5Vmft4OunJhS0fYIuFJceV8cZBLCYIFwwOdboHeBI4uaMWVF1mboWluLCV9Pp6mXae1evUcV9F1XTuaCQuod9VebJTciiQhyLsFQZDZD";
             //   var formData = new MultipartFormDataContent();
             //   formData.Add(new StringContent(campaign.CampaignName), "name");
             //   formData.Add(new StringContent(campaign.Objective), "objective");
             //   formData.Add(new StringContent(campaign.Status), "status");
             //   formData.Add(new StringContent(campaign.SpecialAdCategories.ToString()), "special_ad_categories");
             //   formData.Add(new StringContent(campaign.AccessToken), "access_token");
                

                var response = await _httpClient.PostAsync(url, formData);

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseVM<HttpResponseMessage>("400", "Cannot Create campaign");
                }

                return new ResponseVM<HttpResponseMessage>("200", "Successfully Created campaign", response);
            }
            catch (Exception ex)
            {
                // Log exception here
                Console.WriteLine(ex.ToString());
                return new ResponseVM<HttpResponseMessage>("200", "Cannot Created campaign");

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