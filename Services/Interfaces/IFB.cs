using backend.DTOs;
using backend.Entities;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using static backend.Services.FacebookApiService;

namespace backend.Services.Interfaces
{
    public interface IFB
    {
        Task<ResponseVM<HttpResponseMessage>> CreateCampaign(CampaignDto campaign);
        Task<ResponseVM<AdaccountsDto>> GetAdAccountsData(string accessToken);
        Task<ResponseVM<HttpResponseMessage>> CreateAdSet(AdsetDto adset);
        Task<ResponseVM<string>> ScheduleDelivery(AdDto ad);
        Task<ResponseVM<List<Campaigns>>> GetAllCampaigns();
        Task<ResponseVM<AdCreative>> ProvideAdCreative(AdCreativeDto creative);
        Task<ResponseVM<AdCreative>> CreateAdCreative(AdCreativeDto creative);
        Task<ResponseVM<List<LocationData>>> GetCities(string accessToken, string query);
        Task<ResponseVM<List<Interest>>> GetInterests(string accessToken, string query);
        Task<ResponseVM<List<AdTargetingCategory>>> SearchAdTargetingCategories(string accessToken, string targetType);
        Task<ResponseVM<string>> GetCampaignInsights(string campaignId, string accessToken);
        Task<ResponseVM<string>> GetAdAccountInsights(string adAccountId, string accessToken);

    }
}
