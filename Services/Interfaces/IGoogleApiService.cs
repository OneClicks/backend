using backend.DTOs;
using backend.Entities;
using backend.ViewModels;

namespace backend.Service.Interfaces
{
    public interface IGoogleApiService
    {

        Task<string> GetAllCampaigns(long customerId, string refre);
    }
}
