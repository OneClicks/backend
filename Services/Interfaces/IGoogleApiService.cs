using backend.DTOs;
using backend.Entities;
using backend.ViewModels;

namespace backend.Service.Interfaces
{
    public interface IGoogleApiService
    {
        Task<ResponseVM<string>> GetRefreshToken(string code);
        Task<ResponseVM<string>> RevokeToken(string token);
        Task<string> GetAllCampaigns(long customerId);
        Task<ResponseVM<List<AccountHierarchyDto>>> GetAccountHierarchy(string refreshToken, long? customerId = null);
        Task CreateCampaigns(long customerId);
        Task<ResponseVM<object>> GetAccessibleAccounts(string v);
    }
}
