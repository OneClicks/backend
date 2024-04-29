using backend.DTOs;
using backend.Entities;
using backend.ViewModels;

namespace backend.Service.Interfaces
{
    public interface IGoogleApiService
    {

        Task<string> GetAllCampaigns(long customerId);
        Task<string> GetAccountHierarchy(long? customerId = null);
        Task CreateCampaigns(long customerId);
        void GetAccessibleAccounts();
    }
}
