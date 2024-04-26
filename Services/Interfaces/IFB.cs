using backend.DTOs;
using backend.Entities;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static backend.Service.FacebookApiService;

namespace backend.Service.Interfaces
{
    public interface IFB
    {
        Task<ResponseVM<HttpResponseMessage>> CreateCampaign(CampaignDto campaign);
        Task<ResponseVM<AdaccountsDto>> GetAdAccountsData(string accessToken);
        Task<ResponseVM<string>> CreateAdSet(AdsetDto adset);
        Task<ResponseVM<string>> ScheduleDelivery(string accessToken, string adAccountId, string adsetId, string adsetName, string creativeId);
        Task<ResponseVM<List<Campaigns>>> GetAllCampaigns();
        Task<ResponseVM<string>> GetCities(string query, string accessToken);
        Task<ResponseVM<List<AdTargetingCategory>>> SearchAdTargetingCategories(string accessToken, string apiVersion, string targetType);
    }
}
