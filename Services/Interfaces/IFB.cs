using backend.DTOs;
using backend.Entities;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services.Interfaces
{
    public interface IFB
    {
        Task<ResponseVM<HttpResponseMessage>> CreateCampaignAsync(CampaignDto campaign);
        Task<ResponseVM<AdaccountsDto>> GetAdAccountsData(string accessToken);
        Task<ResponseVM<string>> CreateAdSet(AdsetDto adset);
        Task<ResponseVM<string>> ScheduleDelivery(string accessToken, string adAccountId, string adsetId, string adsetName, string creativeId);
        Task<ResponseVM<List<Campaigns>>> GetAllCampaigns();
        
        }
}
