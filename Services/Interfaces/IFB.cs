using backend.DTOs;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services.Interfaces
{
    public interface IFB
    {
        Task<ResponseVM<HttpResponseMessage>> CreateCampaignAsync(CampaignDto campaign);
        Task<ResponseVM<AdaccountsDto>> GetAdAccountsData(string accessToken);
    }
}
