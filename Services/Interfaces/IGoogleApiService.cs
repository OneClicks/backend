﻿using backend.DTOs.GoogleDtos;
using backend.Entities;
using backend.ViewModels;

namespace backend.Service.Interfaces
{
    public interface IGoogleApiService
    {
        Task<ResponseVM<string>> GetRefreshToken(string code);
        Task<ResponseVM<string>> RevokeToken(string token);
        Task<ResponseVM<List<AccountHierarchyDto>>> GetAccountHierarchy(string refreshToken, long? customerId = null);
        Task<ResponseVM<string>> CreateCampaigns(GoogleCampaignDto campaignDto);
        Task<ResponseVM<object>> GetAccessibleAccounts(string v);
        Task<List<object>> GetAllCampaigns(string refreshToken, long customerId);
        Task<ResponseVM<string>> CreateAdGroup(AdGroupDto adGroupObj);
        Task<ResponseVM<string>> CreateResponsiveSearchAdWithCustomization(AdGroupDto adGroupObj, string adGroupResourceName);
        Task<ResponseVM<string>> AddKeywords(AdGroupDto adGroupObj, string adGroupResourceName);
        Task<ResponseVM<string>> AddGeoTargeting(AdGroupDto adGroupObj );
        Task<ResponseVM<List<object>>> GetAllResponseAds(string refreshToken, long customerId);
    }
}
