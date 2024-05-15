

using backend.DTOs;
using backend.Entities;
using backend.ViewModels;

namespace backend.ServiceFiles.Interfaces
{
    public interface IFB
    {
        Task<ResponseVM<HttpResponseMessage>> CreateCampaign(CampaignDto campaign);
        Task<ResponseVM<AdaccountsDto>> GetAdAccountsData(string accessToken);
        Task<ResponseVM<HttpResponseMessage>> CreateAdSet(AdsetDto adset);
        Task<ResponseVM<List<Adset>>> GetAllAdsets();
        Task<ResponseVM<string>> ScheduleDelivery(AdDto ad);
        Task<ResponseVM<List<Campaigns>>> GetAllCampaigns();
        Task<ResponseVM<object>> GetPayloadForAd(string accessToken, string adAccountId);
        Task<ResponseVM<AdCreative>> CreateAdCreative(AdCreativeDto creative);
        Task<ResponseVM<string>> UploadFile(string accessToken, IFormFile imageFile, string adAccountId);
        Task<ResponseVM<List<LocationData>>> GetCities(string accessToken, string query);
        Task<ResponseVM<List<Interest>>> GetInterests(string accessToken, string query);
        Task<ResponseVM<List<AdCreative>>> GetAllAdcreatives();
        Task<ResponseVM<List<AdTargetingCategory>>> SearchAdTargetingCategories(string accessToken, string targetType);
        Task<ResponseVM<string>> GetCampaignInsights(string campaignId, string accessToken);
        Task<ResponseVM<string>> GetAdAccountInsights(string adAccountId, string accessToken);

        Task<ResponseVM<object>> GetAdSetData(string accessToken, string adAccountId);
        Task<ResponseVM<object>> GetCampaignData(string accessToken, string adAccountId);

    }
}
