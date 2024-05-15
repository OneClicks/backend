using backend.Configurations;
using backend.ServiceFiles.Interfaces;
using backend.ViewModels;
using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V16.Errors;
using Google.Ads.GoogleAds.V16.Resources;
using Google.Ads.GoogleAds.V16.Services;
using Google.Api.Gax;
using static Google.Ads.GoogleAds.V16.Resources.Campaign.Types;
using Google.Ads.GoogleAds.V16.Common;
using static Google.Ads.GoogleAds.V16.Services.SuggestGeoTargetConstantsRequest.Types;
using backend.DTOs.GoogleDtos;
using Google.Ads.GoogleAds;
using Google.Api;
using backend.Helpers;
using backend.Entities;
using backend.Repository.Interfaces;
using static Google.Rpc.Context.AttributeContext.Types;

namespace backend.ServiceFiles
{
    public class GoogleApiService : IGoogleApiService
    {
        private readonly IGenericRepository<RecentActivity> _recentRepository;


        public GoogleApiService( IGenericRepository<RecentActivity> recentRepository)
        {
            _recentRepository = recentRepository;
        }
        #region ACCOUNTS 
        public async Task<ResponseVM<object>> GetAccessibleAccounts(string refreshToken)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = refreshToken
            };
            GoogleAdsClient client = new GoogleAdsClient(config);

            CustomerServiceClient customerService = client.GetService(Services.V16.CustomerService);

            try
            {
                // Retrieve the list of customer resources.

                string[] customerResourceNames = customerService.ListAccessibleCustomers();

                // Display the result.
                foreach (string customerResourceName in customerResourceNames)
                {
                    Console.WriteLine(
                        $"Found customer with resource name = '{customerResourceName}'.");


                }
                return new ResponseVM<object>("200", "", customerResourceNames);
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }
        }
        public async Task<ResponseVM<string>> RevokeToken(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("token", token),
                 });

                // Send the POST request to the revocation endpoint
                HttpResponseMessage response = await client.PostAsync("https://oauth2.googleapis.com/revoke", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Token revoked successfully.");
                    return new ResponseVM<string>("200", "Token revoked successfully.");

                }
                else
                {
                    Console.WriteLine($"Failed to revoke token. Status code: {response.StatusCode}");
                    return new ResponseVM<string>("400", "Cannot revoke refresh token");

                }
            }

        }


        public async Task<ResponseVM<string>> GetRefreshToken(string code)
        {
            //var clientSecrets = new ClientSecrets
            //{
            //    ClientId = Constants.GoogleClientId,
            //    ClientSecret = Constants.GoogleClientSecret
            //};

            //var tokenRequestContent = new MultipartFormDataContent
            //{
            //    { new StringContent(code), "code" },
            //    { new StringContent(clientSecrets.ClientId), "client_id" },
            //    { new StringContent(clientSecrets.ClientSecret), "client_secret" },
            //    { new StringContent("https://localhost:3000"), "redirect_uri" },
            //    { new StringContent("authorization_code"), "grant_type" }
            //};

            //var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequestContent);
            //var responseContent = await response.Content.ReadAsStringAsync();

            //using JsonDocument document = JsonDocument.Parse(responseContent);

            //var res = document.RootElement.GetProperty("refresh_token").GetString();

            var res = "1//03v7pNMJs1LOPCgYIARAAGAMSNwF-L9IrDpDmkd1-ga1Y6jAaYrYtfqi6Re3xy31rPhoVQvl7OgAuTDgmkdxnsqHV7kCERZ-WuNc";
            return new ResponseVM<string>("200", "Successfully fetched refresh token", res);
        }

        public async Task<ResponseVM<List<AccountHierarchyDto>>> GetAccountHierarchy(string refreshToken, long? customerId = null)
        {
            var accounts = new List<AccountHierarchyDto>();
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = refreshToken
            };

            GoogleAdsClient client = new GoogleAdsClient(config);

            GoogleAdsServiceClient googleAdsServiceClient =
                client.GetService(Services.V16.GoogleAdsService);

            CustomerServiceClient customerServiceClient =
                client.GetService(Services.V16.CustomerService);

            List<long> seedCustomerIds = new List<long>();

            if (customerId.HasValue)
            {
                seedCustomerIds.Add(customerId.Value);
            }
            else
            {
                string[] customerResourceNames = customerServiceClient.ListAccessibleCustomers();

                foreach (string customerResourceName in customerResourceNames)
                {
                    CustomerName customerName = CustomerName.Parse(customerResourceName);
                    Console.WriteLine(customerName.CustomerId);
                    seedCustomerIds.Add(long.Parse(customerName.CustomerId));
                }

                Console.WriteLine();
            }

            const string query = @"SELECT
                                customer_client.client_customer,
                                customer_client.level,
                                customer_client.manager,
                                customer_client.descriptive_name,
                                customer_client.currency_code,
                                customer_client.time_zone,
                                customer_client.id
                            FROM customer_client
                            WHERE
                                customer_client.level <= 1";

            Dictionary<long, List<CustomerClient>> customerIdsToChildAccounts =
                new Dictionary<long, List<CustomerClient>>();

            foreach (long seedCustomerId in seedCustomerIds)
            {
                Queue<long> unprocessedCustomerIds = new Queue<long>();
                unprocessedCustomerIds.Enqueue(seedCustomerId);
                CustomerClient rootCustomerClient = null;

                while (unprocessedCustomerIds.Count > 0)
                {
                    long managerCustomerId = unprocessedCustomerIds.Dequeue();
                    PagedEnumerable<SearchGoogleAdsResponse, GoogleAdsRow> response;
                    try
                    {
                        response =
                        googleAdsServiceClient.Search(
                            managerCustomerId.ToString(),
                            query,
                            pageSize: PAGE_SIZE
                        );

                        foreach (GoogleAdsRow googleAdsRow in response)
                        {
                            CustomerClient customerClient = googleAdsRow.CustomerClient;

                            if (customerClient.Level == 0)
                            {
                                if (rootCustomerClient == null)
                                {
                                    rootCustomerClient = customerClient;
                                }

                                continue;
                            }

                            if (!customerIdsToChildAccounts.ContainsKey(managerCustomerId))
                                customerIdsToChildAccounts.Add(managerCustomerId,
                                    new List<CustomerClient>());

                            customerIdsToChildAccounts[managerCustomerId].Add(customerClient);

                            if (customerClient.Manager)
                                if (!customerIdsToChildAccounts.ContainsKey(customerClient.Id) &&
                                    customerClient.Level == 1)
                                    unprocessedCustomerIds.Enqueue(customerClient.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return new ResponseVM<List<AccountHierarchyDto>>("400", "Cannot use Standard on TEST ACCESS BASIS. Select another account");

                        throw;
                    }


                }

                if (rootCustomerClient != null)
                {
                    Console.WriteLine("The hierarchy of customer ID {0} is printed below:",
                        rootCustomerClient.Id);
                    var account = await PrintAccountHierarchy(rootCustomerClient, customerIdsToChildAccounts, 0);
                    if (account != null)
                    {
                        accounts.Add(account);
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine(
                        "Customer ID {0} is likely a test account, so its customer client " +
                        " information cannot be retrieved.");
                }
            }

            return new ResponseVM<List<AccountHierarchyDto>>("200", "Successfully fetched data of accounts", accounts);

        }

        private const int PAGE_SIZE = 1000;

        private async Task<AccountHierarchyDto> PrintAccountHierarchy(CustomerClient customerClient, Dictionary<long, List<CustomerClient>> customerIdsToChildAccounts, int depth)
        {
            if (customerClient == null)
            {
                return null;
            }

            long customerId = customerClient.Id;
            var manager = new AccountHierarchyDto();

            if (depth == 0)
            {
                manager.CustomerId = customerId;
                manager.CurrencyCode = customerClient.CurrencyCode;
                manager.DescriptiveName = customerClient.DescriptiveName;
                manager.TimeZone = customerClient.TimeZone;
                manager.ChildAccounts = new List<AccountHierarchyDto>();
            }

            if (customerIdsToChildAccounts.ContainsKey(customerId))
            {
                foreach (CustomerClient childAccount in customerIdsToChildAccounts[customerId])
                {
                    var childDto = new AccountHierarchyDto
                    {
                        CustomerId = childAccount.Id,
                        CurrencyCode = childAccount.CurrencyCode,
                        DescriptiveName = childAccount.DescriptiveName,
                        TimeZone = childAccount.TimeZone
                    };
                    if (childDto != null)
                    {
                        manager.ChildAccounts.Add(childDto);
                    }
                }
            }

            return manager;
        }

        public async Task<ResponseVM<string>> CreateCustomer(string refreshToken, long customerId)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = refreshToken,
                //"1//03v7pNMJs1LOPCgYIARAAGAMSNwF-L9IrDpDmkd1-ga1Y6jAaYrYtfqi6Re3xy31rPhoVQvl7OgAuTDgmkdxnsqHV7kCERZ-WuNc",
                LoginCustomerId = Constants.GoogleCustomerId.ToString()
            };

            GoogleAdsClient client = new GoogleAdsClient(config);

            // Get the GoogleAdsService.

            var googleAdsService = client.GetService(Services.V16.CustomerService);

            // Create a query that will retrieve all campaigns.

            Customer customer = new Customer()
            {
                DescriptiveName = $"Account created with CustomerService on '{DateTime.Now}'",

                // For a list of valid currency codes and time zones see this documentation:
                // https://developers.google.com/google-ads/api/reference/data/codes-formats#codes_formats.
                CurrencyCode = "USD",
                TimeZone = "America/New_York",

                // The below values are optional. For more information about URL
                // options see: https://support.google.com/google-ads/answer/6305348.
                TrackingUrlTemplate = "{lpurl}?device={device}",
                FinalUrlSuffix = "keyword={keyword}&matchtype={matchtype}&adgroupid={adgroupid}"
            };

            try
            {
                // Create the account.
                CreateCustomerClientResponse response = googleAdsService.CreateCustomerClient(
                     Constants.GoogleCustomerId.ToString(), customer);
                var temp = new RecentActivity
                {
                    Activity = "New Customer Created: Google",
                    DateTime = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt")
                };
                await _recentRepository.Create(temp);

                // Display the result.

                return new ResponseVM<string>("200", $"Created a customer with resource name " +
                    $"'{response.ResourceName}' under the manager account with customer " +
                    $"ID '{Constants.GoogleCustomerId.ToString()}'", response.ResourceName);
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }

        }
        #endregion

        #region CAMPAIGNS
        public async Task<List<object>> GetAllCampaigns(string refreshToken, long customerId)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = refreshToken,
                //"1//03v7pNMJs1LOPCgYIARAAGAMSNwF-L9IrDpDmkd1-ga1Y6jAaYrYtfqi6Re3xy31rPhoVQvl7OgAuTDgmkdxnsqHV7kCERZ-WuNc",
                LoginCustomerId = Constants.GoogleCustomerId.ToString()
            };

            GoogleAdsClient client = new GoogleAdsClient(config);
            var googleAdsService = client.GetService(Services.V16.GoogleAdsService);

            string query = @"SELECT
                                campaign.id,
                                campaign.name,
                                campaign.status,
                                campaign.manual_cpc.enhanced_cpc_enabled,
                                campaign.start_date,
                                campaign.campaign_budget,
                                campaign.end_date
                                FROM campaign
                                ORDER BY campaign.id";

            try
            {
                List<object> campaignDetails = new List<object>();
                googleAdsService.SearchStream(customerId.ToString(), query,
                    async delegate (SearchGoogleAdsStreamResponse resp)
                    {
                        foreach (GoogleAdsRow googleAdsRow in resp.Results)
                        {

                                var details = new
                                {
                                    CampaignId = googleAdsRow.Campaign.Id.ToString(),
                                    CampaignName = googleAdsRow.Campaign.Name,
                                    ManualCpc = googleAdsRow.Campaign.ManualCpc.EnhancedCpcEnabled ? "Enhanced Cpc Enabled" : "",
                                    Status = GoogleMapper.CampaignStatusToString(googleAdsRow.Campaign.Status),
                                    //Budget = googleAdsRow.Campaign.CampaignBudget,
                                    StartDate = googleAdsRow.Campaign.StartDate,
                                    EndDate = googleAdsRow.Campaign.EndDate,
                                    Budget = await GetCampaignBudget(customerId.ToString(), Constants.GoogleCustomerId.ToString(),googleAdsRow.Campaign.CampaignBudget, refreshToken),
                                    Type = "Google"

                                };

                                campaignDetails.Add(details);
                        }   
                    }
                );
                return campaignDetails;
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }

        }

        #region OLD CODE WITHOUT AWAIT 
        //public async Task<ResponseVM<string>> CreateCampaigns(GoogleCampaignDto campaignDto)
        //{
        //    GoogleAdsConfig config = new GoogleAdsConfig()
        //    {
        //        DeveloperToken = Constants.GoogleDeveloperToken,
        //        OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
        //        OAuth2ClientId = Constants.GoogleClientId,
        //        OAuth2ClientSecret = Constants.GoogleClientSecret,
        //        OAuth2RefreshToken = campaignDto.RefreshToken,                //"1//03v7pNMJs1LOPCgYIARAAGAMSNwF-L9IrDpDmkd1-ga1Y6jAaYrYtfqi6Re3xy31rPhoVQvl7OgAuTDgmkdxnsqHV7kCERZ-WuNc",
        //        LoginCustomerId =campaignDto.CustomerId.ToString()           //Constants.GoogleCustomerId.ToString()
        //    };

        //    GoogleAdsClient client = new GoogleAdsClient(config);
        //    CampaignServiceClient campaignService = client.GetService(Services.V16.CampaignService);

        //    // Create a budget to be used for the campaign.
        //    string budget = CreateBudget(client, campaignDto);

        //    List<CampaignOperation> operations = new List<CampaignOperation>();

        //    Campaign campaign = new Campaign()
        //    {
        //        Name = campaignDto.CampaignName,
        //        AdvertisingChannelType = GoogleMapper.MapToEnum(campaignDto.AdvertisingChannelType),

        //        // Recommendation: Set the campaign to PAUSED when creating it to prevent
        //        // the ads from immediately serving. Set to ENABLED once you've added
        //        // targeting and the ads are ready to serve
        //        Status = GoogleMapper.CampaignStatusMapper(campaignDto.Status),

        //        // Set the bidding strategy and budget.
        //        ManualCpc = new ManualCpc(),
        //        CampaignBudget = budget,

        //        // Set the campaign network options.
        //        NetworkSettings = new NetworkSettings
        //        {
        //            TargetGoogleSearch = true,
        //            TargetSearchNetwork = true,
        //            // Enable Display Expansion on Search campaigns. See
        //            // https://support.google.com/google-ads/answer/7193800 to learn more.
        //            TargetContentNetwork = true,
        //            TargetPartnerSearchNetwork = false
        //        },

        //        // Optional: Set the start date.
        //        StartDate = campaignDto.StartDate,

        //        // Optional: Set the end date.
        //        EndDate = campaignDto.EndDate,
        //    };

        //    // Create the operation.
        //    operations.Add(new CampaignOperation() { Create = campaign });

        //    try
        //    {
        //        // Add the campaigns.
        //        MutateCampaignsResponse retVal = campaignService.MutateCampaigns(
        //            campaignDto.CustomerId.ToString(), operations);

        //        // Display the results.
        //        if (retVal.Results.Count > 0)
        //        {
        //            foreach (MutateCampaignResult newCampaign in retVal.Results)
        //            {
        //                Console.WriteLine("Campaign with resource ID = '{0}' was added.",
        //                    newCampaign.ResourceName);
        //            }
        //            return new ResponseVM<string>("200", "Successfully created campaign");
        //        }
        //        else
        //        {
        //            Console.WriteLine("No campaigns were added.");
        //            return new ResponseVM<string>("400", "Failed to create campaign");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Failure:");
        //        Console.WriteLine($"Message: {e.Message}");


        //        throw new Exception($"Google Ads API request failed: {e.Message}", e);
        //    }

        //}
        //private static string CreateBudget(GoogleAdsClient client, GoogleCampaignDto dto)
        //{
        //    // Get the BudgetService.
        //    CampaignBudgetServiceClient budgetService = client.GetService(
        //        Services.V16.CampaignBudgetService);

        //    // Create the campaign budget.
        //    CampaignBudget budget = new CampaignBudget()
        //    {
        //        Name = dto.BudgetName,
        //        DeliveryMethod = GoogleMapper.BudgetDeliveryMethodMapper(dto.BudgetDeliveryMethod),
        //        AmountMicros = (long)(double.Parse(dto.BudgetAmount) * 1_000_000),
        //    };

        //    // Create the operation.
        //    CampaignBudgetOperation budgetOperation = new CampaignBudgetOperation()
        //    {
        //        Create = budget
        //    };

        //    // Create the campaign budget.
        //    MutateCampaignBudgetsResponse response = budgetService.MutateCampaignBudgets(
        //        dto.CustomerId.ToString(), new CampaignBudgetOperation[] { budgetOperation });
        //    return response.Results[0].ResourceName;
        //}
        #endregion
        public async Task<ResponseVM<string>> CreateCampaigns(GoogleCampaignDto campaignDto)
        {
            try
            {
                GoogleAdsConfig config = new GoogleAdsConfig()
                {
                    DeveloperToken = Constants.GoogleDeveloperToken,
                    OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                    OAuth2ClientId = Constants.GoogleClientId,
                    OAuth2ClientSecret = Constants.GoogleClientSecret,
                    OAuth2RefreshToken = campaignDto.RefreshToken,
                    LoginCustomerId = campaignDto.ManagerId.ToString()
                };

                GoogleAdsClient client = new GoogleAdsClient(config);
                CampaignServiceClient campaignService = client.GetService(Services.V16.CampaignService);

                // Create a budget to be used for the campaign.
                string budget = await CreateBudget(campaignDto);

                List<CampaignOperation> operations = new List<CampaignOperation>();

                Campaign campaign = new Campaign()
                {
                    Name = campaignDto.CampaignName,
                    AdvertisingChannelType = GoogleMapper.MapToEnum(campaignDto.AdvertisingChannelType),
                    Status = GoogleMapper.CampaignStatusMapper(campaignDto.Status),
                    ManualCpc = new ManualCpc(),
                    CampaignBudget = budget,
                    NetworkSettings = new NetworkSettings
                    {
                        TargetGoogleSearch = campaignDto.TargetGoogleSearch,
                        TargetSearchNetwork = campaignDto.TargetGoogleSearch,
                        TargetContentNetwork = true,
                        TargetPartnerSearchNetwork = false
                    },
                    StartDate = campaignDto.StartDate,
                    EndDate = campaignDto.EndDate,
                };

                operations.Add(new CampaignOperation() { Create = campaign });

                // Add the campaigns.
                MutateCampaignsResponse retVal = await campaignService.MutateCampaignsAsync(
                    campaignDto.CustomerId.ToString(), operations);

                // Display the results.
                if (retVal.Results.Count > 0)
                {
                    var temp = new RecentActivity
                    {
                        Activity = "New Camaign added : Google",
                        DateTime = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt")
                    };
                    await _recentRepository.Create(temp);

                    foreach (MutateCampaignResult newCampaign in retVal.Results)
                    {
                        Console.WriteLine("Campaign with resource ID = '{0}' was added.",
                            newCampaign.ResourceName);
                    }
                    return new ResponseVM<string>("200", "Successfully created campaign");
                }
                else
                {
                    Console.WriteLine("No campaigns were added.");
                    return new ResponseVM<string>("400", "Failed to create campaign");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                throw new Exception($"Google Ads API request failed: {e.Message}", e);
            }
        }
        private async Task<string> GetCampaignBudget(string customerId, string managerId,string campaignId, string refreshToken)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = refreshToken, // Replace with your refresh token
                LoginCustomerId = managerId.ToString() // Replace with your manager ID
            };

            GoogleAdsClient client = new GoogleAdsClient(config);

            string query = $@"SELECT
                        campaign_budget.id,
                        campaign_budget.amount_micros
                          FROM campaign_budget
                          WHERE campaign_budget.id = '{campaignId.Split('/').LastOrDefault()}'";

            try
            {
                List<object> budgetDetails = new List<object>();
                GoogleAdsServiceClient googleAdsService = client.GetService(Services.V16.GoogleAdsService);
                googleAdsService.SearchStream(customerId, query,
                    delegate (SearchGoogleAdsStreamResponse resp)
                    {
                        foreach (GoogleAdsRow googleAdsRow in resp.Results)
                        {
                            var budget = new
                            {
                                AmountMicros = googleAdsRow.CampaignBudget.AmountMicros
                            };

                            budgetDetails.Add(budget.AmountMicros);
                        }
                    }
                );

                // Assuming you only want the first result (there should be only one budget per campaign)
                if (budgetDetails.Count > 0)
                {
                    return budgetDetails[0].ToString();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                throw new Exception($"Google Ads API request failed: {e.Message}", e);
            }
        }

        private  async Task<string> CreateBudget( GoogleCampaignDto dto)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = dto.RefreshToken,
                LoginCustomerId = dto.ManagerId.ToString()
            };

            GoogleAdsClient client = new GoogleAdsClient(config);

            CampaignBudgetServiceClient budgetService = client.GetService(
                Services.V16.CampaignBudgetService);

            CampaignBudget budget = new CampaignBudget()
            {
                Name = dto.BudgetName,
                DeliveryMethod = GoogleMapper.BudgetDeliveryMethodMapper(dto.BudgetDeliveryMethod),
                AmountMicros = (long)(double.Parse(dto.BudgetAmount) * 1_000_000),
            };

            CampaignBudgetOperation budgetOperation = new CampaignBudgetOperation()
            {
                Create = budget
            };

            try
            {

                MutateCampaignBudgetsResponse response = await budgetService.MutateCampaignBudgetsAsync(
                    dto.CustomerId.ToString(), new CampaignBudgetOperation[] { budgetOperation });
                var temp = new RecentActivity
                {
                    Activity = "New Budget Created for Campaign: Google",
                    DateTime = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt")
                };
                 await _recentRepository.Create(temp);

                return response.Results[0].ResourceName;

            }
            catch (Exception e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                throw new Exception($"Google Ads API request failed: {e.Message}", e);
            }

        }
        #endregion

        #region ADS GROUPS
        public async Task<ResponseVM<string>> CreateAdGroup(AdGroupDto adGroupObj)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = adGroupObj.RefreshToken,
                LoginCustomerId = adGroupObj.ManagerId.ToString()
            };
            GoogleAdsClient client = new GoogleAdsClient(config);

            AdGroupServiceClient adGroupService = client.GetService(Services.V16.AdGroupService);

            List<AdGroupOperation> operations = new List<AdGroupOperation>();
            AdGroup adGroup = new AdGroup()
            {
                Name = adGroupObj.AdGroupName,
                Status = GoogleMapper.AdGroupStatusMapper(adGroupObj.AdGroupStatus),
                Campaign = ResourceNames.Campaign(adGroupObj.CustomerId, adGroupObj.CampaignId),
                CpcBidMicros = (long)(double.Parse(adGroupObj.AdGroupBidAmount) * 1_000_000),

            };
            AdGroupOperation operation = new AdGroupOperation()
            {
                Create = adGroup
            };
            operations.Add(operation);
            try
            {
                // Create the ad groups.
                MutateAdGroupsResponse response = await adGroupService.MutateAdGroupsAsync(
                    adGroupObj.CustomerId.ToString(), operations);
                string adGroupResourceName = response.Results[0].ResourceName; //name of ad group
                // Display the results.
                var temp = new RecentActivity
                {
                    Activity = "New AdGroup added : Google",
                    DateTime = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt")
                };
                await _recentRepository.Create(temp);

                foreach (MutateAdGroupResult newAdGroup in response.Results)
                {
                    Console.WriteLine("Ads group with resource name '{0}' was created.",
                        newAdGroup.ResourceName);
                }
                string[] parts = adGroupResourceName.Split('/');
                long adGroupId = long.Parse(parts.Last());

               // string customizedAttribute = GetCustomizedAttribute(adGroupObj, adGroupId, adGroupResourceName);

                var searchAdobj = CreateResponsiveSearchAdWithCustomization(adGroupObj, adGroupResourceName);
                var keywordsObj = AddKeywords(adGroupObj, adGroupResourceName);
                var geotargetingObj = AddGeoTargeting(adGroupObj);

                if( keywordsObj.Result.StatusCode == "200" && geotargetingObj.Result.StatusCode == "200") 
                {
                    return new ResponseVM<string>("200", "Successfully created Ads group ", adGroupResourceName);
                }
                else
                {
                    return new ResponseVM<string>("400", "Failed to create Ads group ", adGroupResourceName);
                }
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }
        }
        #endregion

        #region customizer
        /*   #region GetCustomizedAttribute
           private string GetCustomizedAttribute(AdGroupDto adGroupObj,long adGroupId, string adGroupResourceName)
           {
               try
               {
                   string textCustomizerAttributeResourceName = CreateTextCustomizerAttribute(adGroupObj, adGroupObj.SearchAds.CustomizerAttributeName);
                   string priceCustomizerAttributeResourceName = CreatePriceCustomizerAttribute(adGroupObj, adGroupObj.SearchAds.CustomizerAttributePrice);

                   LinkCustomizerAttributes(adGroupObj, adGroupId, textCustomizerAttributeResourceName, priceCustomizerAttributeResourceName);

                   // Create an ad with the customizations provided by the ad customizer attributes.
                   var searchAdobj = CreateResponsiveSearchAdWithCustomization(adGroupObj, adGroupResourceName);

                   return searchAdobj.Result.ResponseData.ToString();
               }
               catch (GoogleAdsException e)
               {
                   Console.WriteLine("Failure:");
                   Console.WriteLine($"Message: {e.Message}");
                   Console.WriteLine($"Failure: {e.Failure}");
                   Console.WriteLine($"Request ID: {e.RequestId}");
                   throw;
               }
           }
           private string CreateTextCustomizerAttribute(AdGroupDto adGroupObj, string customizerName)
           {
               GoogleAdsConfig config = new GoogleAdsConfig()
               {
                   DeveloperToken = Constants.GoogleDeveloperToken,
                   OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                   OAuth2ClientId = Constants.GoogleClientId,
                   OAuth2ClientSecret = Constants.GoogleClientSecret,
                   OAuth2RefreshToken = adGroupObj.RefreshToken,
                   LoginCustomerId = adGroupObj.ManagerId.ToString()
               };
               GoogleAdsClient client = new GoogleAdsClient(config);

               CustomizerAttributeServiceClient customizerAttributeService = client.GetService(Services.V16.CustomizerAttributeService);

               CustomizerAttribute textAttribute = new CustomizerAttribute()
               {
                   Name = customizerName,
                   Type = CustomizerAttributeType.Text
               };

               CustomizerAttributeOperation textAttributeOperation = new CustomizerAttributeOperation()
               {
                   Create = textAttribute
               };

               MutateCustomizerAttributesResponse response = customizerAttributeService.MutateCustomizerAttributes(adGroupObj.CustomerId.ToString(),
                       new[] { textAttributeOperation });

               string customizerAttributeResourceName = response.Results[0].ResourceName;
               Console.WriteLine($"Added text customizer attribute with resource name" +$" '{customizerAttributeResourceName}'.");

               return customizerAttributeResourceName;
           }
           private string CreatePriceCustomizerAttribute(AdGroupDto adGroupObj, string customizerName)
           {
               GoogleAdsConfig config = new GoogleAdsConfig()
               {
                   DeveloperToken = Constants.GoogleDeveloperToken,
                   OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                   OAuth2ClientId = Constants.GoogleClientId,
                   OAuth2ClientSecret = Constants.GoogleClientSecret,
                   OAuth2RefreshToken = adGroupObj.RefreshToken,
                   LoginCustomerId = adGroupObj.ManagerId.ToString()
               };
               GoogleAdsClient client = new GoogleAdsClient(config);

               CustomizerAttributeServiceClient customizerAttributeService = client.GetService(Services.V16.CustomizerAttributeService);

               CustomizerAttribute priceAttribute = new CustomizerAttribute()
               {
                   Name = customizerName,
                   Type = CustomizerAttributeType.Price
               };

               CustomizerAttributeOperation priceAttributeOperation = new CustomizerAttributeOperation()
               {
                   Create = priceAttribute
               };

               MutateCustomizerAttributesResponse response = customizerAttributeService.MutateCustomizerAttributes(adGroupObj.CustomerId.ToString(),
                       new[] { priceAttributeOperation });

               string customizerAttributeResourceName = response.Results[0].ResourceName;
               Console.WriteLine($"Added price customizer attribute with resource name" +$" '{customizerAttributeResourceName}'.");

               return customizerAttributeResourceName;
           }
           private void LinkCustomizerAttributes(AdGroupDto adGroupObj, long adGroupId, string textCustomizerAttributeResourceName, string priceCustomizerAttributeResourceName)
           {
               GoogleAdsConfig config = new GoogleAdsConfig()
               {
                   DeveloperToken = Constants.GoogleDeveloperToken,
                   OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                   OAuth2ClientId = Constants.GoogleClientId,
                   OAuth2ClientSecret = Constants.GoogleClientSecret,
                   OAuth2RefreshToken = adGroupObj.RefreshToken,
                   LoginCustomerId = adGroupObj.ManagerId.ToString()
               };
               GoogleAdsClient client = new GoogleAdsClient(config);
               AdGroupCustomizerServiceClient adGroupCustomizerService = client.GetService(Services.V16.AdGroupCustomizerService);

               List<AdGroupCustomizerOperation> adGroupCustomizerOperations = new List<AdGroupCustomizerOperation>();

               AdGroupCustomizer marsCustomizer = new AdGroupCustomizer()
               {
                   CustomizerAttribute = textCustomizerAttributeResourceName,
                   Value = new CustomizerValue()
                   {
                       Type = CustomizerAttributeType.Text,
                       StringValue = "Mars"
                   },
                   AdGroup = ResourceNames.AdGroup(adGroupObj.CustomerId, adGroupId)
               };

               adGroupCustomizerOperations.Add(new AdGroupCustomizerOperation()
               {
                   Create = marsCustomizer
               });

               AdGroupCustomizer priceCustomizer = new AdGroupCustomizer()
               {
                   CustomizerAttribute = priceCustomizerAttributeResourceName,
                   Value = new CustomizerValue()
                   {
                       Type = CustomizerAttributeType.Price,
                       StringValue = "100.0€"
                   },
                   AdGroup = ResourceNames.AdGroup(adGroupObj.CustomerId, adGroupId)
               };

               adGroupCustomizerOperations.Add(new AdGroupCustomizerOperation()
               {
                   Create = priceCustomizer
               });

               MutateAdGroupCustomizersResponse response = adGroupCustomizerService.MutateAdGroupCustomizers(adGroupObj.CustomerId.ToString(),
                       adGroupCustomizerOperations);

               foreach (MutateAdGroupCustomizerResult result in response.Results)
               {
                   Console.WriteLine($"Added an ad group customizer with resource name '{result.ResourceName}'.");
               }
           }

           #endregion*/
        #endregion

        #region  RESPONSIVE SEARCH ADS
        public async Task<ResponseVM<string>> CreateResponsiveSearchAdWithCustomization(AdGroupDto adGroupObj, string adGroupResourceName)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = adGroupObj.RefreshToken,
                LoginCustomerId = adGroupObj.ManagerId.ToString()
            };

            AdGroupAdOperation operation = new AdGroupAdOperation()
            {
                Create = new AdGroupAd()
                {
                    AdGroup = adGroupResourceName,
                    
                    Status = GoogleMapper.AdGroupAdStatusMapper(adGroupObj.AdGroupStatus),

                    Ad = new Ad()
                    {
                        Name = adGroupObj.AdName,
                        ResponsiveSearchAd = new ResponsiveSearchAdInfo()
                        {
                            Headlines =
                            {
                                new AdTextAsset() { Text = adGroupObj.SearchAds.Headlines[0] },
                                new AdTextAsset() { Text = adGroupObj.SearchAds.Headlines[1] },
                                new AdTextAsset() { Text = adGroupObj.SearchAds.Headlines[2] }
                            },

                            Descriptions =
                            {
                                new AdTextAsset() { Text =  adGroupObj.SearchAds.Descriptions[0] },
                                new AdTextAsset() { Text =  adGroupObj.SearchAds.Descriptions[1] }                                 
                            },
                         
                            Path1 = adGroupObj.SearchAds.Path1,
                            Path2 = adGroupObj.SearchAds.Path2
                        },

                        FinalUrls = { adGroupObj.SearchAds.TargetUrl }
                    }
                }
            };
            GoogleAdsClient client = new GoogleAdsClient(config);

            AdGroupAdServiceClient serviceClient = client.GetService(Services.V16.AdGroupAdService);
            try
            {
                MutateAdGroupAdsResponse response = serviceClient.MutateAdGroupAds(adGroupObj.CustomerId.ToString(),
                    new[] { operation }.ToList());
                var temp = new RecentActivity
                {
                    Activity = "New Ad Created : Google",
                    DateTime = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt")
                };
                await _recentRepository.Create(temp);

                string resourceName = response.Results[0].ResourceName;
                return new ResponseVM<string>("200", "Successfully created responsive search ad with resource name: ", resourceName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new ResponseVM<string>("500", "Error occurred: " + ex.Message, null);
            }
           
        }

        public async Task<ResponseVM<string>> AddKeywords(AdGroupDto adGroupObj, string adGroupResourceName)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = adGroupObj.RefreshToken,
                LoginCustomerId = adGroupObj.ManagerId.ToString()
            };

            GoogleAdsClient client = new GoogleAdsClient(config);
            AdGroupCriterionServiceClient adGroupCriterionService = client.GetService(Services.V16.AdGroupCriterionService);

            List<AdGroupCriterionOperation> operations = new List<AdGroupCriterionOperation>();

            AdGroupCriterionOperation exactMatchOperation = new AdGroupCriterionOperation()
            {
                Create = new AdGroupCriterion()
                {
                    AdGroup = adGroupResourceName,
                    Status = GoogleMapper.AdGroupCriterionStatusMapper(adGroupObj.AdGroupStatus),
                    Keyword = new KeywordInfo()
                    {
                        Text = adGroupObj.Keywords.Keywords,
                        MatchType = GoogleMapper.KeywordMatchTypeMapper("Exact")
                    },
                    Negative = adGroupObj.Keywords.Negative,
                }
            };
            operations.Add(exactMatchOperation);

            AdGroupCriterionOperation phraseMatchOperation = new AdGroupCriterionOperation()
            {
                Create = new AdGroupCriterion()
                {
                    AdGroup = adGroupResourceName,
                    Status = GoogleMapper.AdGroupCriterionStatusMapper(adGroupObj.AdGroupStatus),
                    Keyword = new KeywordInfo()
                    {
                        Text = adGroupObj.Keywords.Keywords,
                        MatchType = GoogleMapper.KeywordMatchTypeMapper("Phrase")
                    },
                    Negative = adGroupObj.Keywords.Negative,
                }
            };
            operations.Add(phraseMatchOperation);

            AdGroupCriterionOperation broadMatchOperation = new AdGroupCriterionOperation()
            {
                Create = new AdGroupCriterion()
                {
                    AdGroup = adGroupResourceName,
                    Status = GoogleMapper.AdGroupCriterionStatusMapper(adGroupObj.AdGroupStatus),
                    Keyword = new KeywordInfo()
                    {
                        Text = adGroupObj.Keywords.Keywords,
                        MatchType = GoogleMapper.KeywordMatchTypeMapper("Broad")
                    },
                        Negative = adGroupObj.Keywords.Negative
                    
                }
            };
            operations.Add(broadMatchOperation);

            MutateAdGroupCriteriaResponse response = await
                adGroupCriterionService.MutateAdGroupCriteriaAsync(adGroupObj.CustomerId.ToString(), operations);

            foreach (MutateAdGroupCriterionResult newAdGroupCriterion in response.Results)
            {
                Console.WriteLine("Keyword with resource name '{0}' was created.",
                    newAdGroupCriterion.ResourceName);
            }
            return new ResponseVM<string>("200", "Successfuly created Keyword with resource name: ", response.Results[0].ResourceName);
        }

        public async Task<ResponseVM<string>> AddGeoTargeting(AdGroupDto adGroupObj)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = adGroupObj.RefreshToken,
                LoginCustomerId = adGroupObj.ManagerId.ToString()
            };

            GoogleAdsClient client = new GoogleAdsClient(config);
            GeoTargetConstantServiceClient geoTargetConstantService = client.GetService(Services.V16.GeoTargetConstantService);

            var campaignName =await  GetAllCampaigns(adGroupObj.RefreshToken, adGroupObj.CustomerId);

            SuggestGeoTargetConstantsRequest suggestGeoTargetConstantsRequest = new SuggestGeoTargetConstantsRequest()
            {
                // Locale uses the ISO 639-1 format.
                Locale = "es",
                CountryCode = adGroupObj.GeoTargeting.CountryCode,
                LocationNames = new LocationNames()
                {
                    Names = { adGroupObj.GeoTargeting.CityName}
                }
            };

            SuggestGeoTargetConstantsResponse suggestGeoTargetConstantsResponse = geoTargetConstantService.SuggestGeoTargetConstants(suggestGeoTargetConstantsRequest);

            List<CampaignCriterionOperation> operations = new List<CampaignCriterionOperation>();
            foreach (GeoTargetConstantSuggestion suggestion in suggestGeoTargetConstantsResponse.GeoTargetConstantSuggestions)
            {
                Console.WriteLine($"Geo target constant: {suggestion.GeoTargetConstant.Name} was " +
                $"found in locale ({suggestion.Locale}) with reach ({suggestion.Reach}) from " +
                $"search term ({suggestion.SearchTerm})");

                CampaignCriterionOperation operation = new CampaignCriterionOperation()
                {
                    Create = new CampaignCriterion()
                    {
                        Campaign = ResourceNames.Campaign(adGroupObj.CustomerId, adGroupObj.CampaignId),
                        Location = new LocationInfo()
                        {
                            GeoTargetConstant = suggestion.GeoTargetConstant.ResourceName
                        }
                    }
                };
                operations.Add(operation);
            }

            CampaignCriterionServiceClient campaignCriterionService = client.GetService(Services.V16.CampaignCriterionService);
            try
            {
                MutateCampaignCriteriaResponse mutateCampaignCriteriaResponse = campaignCriterionService.MutateCampaignCriteria(adGroupObj.CustomerId.ToString(), operations);
                foreach (MutateCampaignCriterionResult result in mutateCampaignCriteriaResponse.Results)
                {
                    Console.WriteLine($"Added campaign criterion {result.ResourceName}");
                }
                return new ResponseVM<string>("200", "Successfuly Added campaign criterion: ", mutateCampaignCriteriaResponse.Results[0].ResourceName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new ResponseVM<string>("500", "Error occurred: " + ex.Message, null);
            }


        
        }

        public async Task<ResponseVM<List<object>>> GetAllResponseAds(string refreshToken, long customerId, long managerId)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = refreshToken,
                LoginCustomerId = managerId.ToString()
            };

            GoogleAdsClient client = new GoogleAdsClient(config);
            var googleAdsService = client.GetService(Services.V16.GoogleAdsService);
          
            string query = @"SELECT
                            ad_group.id,
                            ad_group_ad.resource_name,
                            ad_group.name,
                            ad_group_ad.ad.name,
                            ad_group_ad.ad.id,
                            ad_group_ad.ad.responsive_search_ad.headlines,
                            ad_group_ad.ad.responsive_search_ad.descriptions,
                            ad_group_ad.status
                        FROM ad_group_ad
                        WHERE ad_group_ad.ad.type = RESPONSIVE_SEARCH_AD
                        AND ad_group_ad.status != 'REMOVED'";

            try
            {
                List<object> AdDetails = new List<object>();
                googleAdsService.SearchStream(customerId.ToString(), query,
                    delegate (SearchGoogleAdsStreamResponse resp)
                    {
                        foreach (GoogleAdsRow googleAdsRow in resp.Results)
                        {
                            var details = new
                            {

                                AdId = googleAdsRow.AdGroupAd.Ad.Id,
                                AdGroupName = googleAdsRow.AdGroup.Name,
                                AdResourceName = googleAdsRow.AdGroupAd.ResourceName,
                                AdName = googleAdsRow.AdGroupAd.Ad.Name,
                                Headlines = googleAdsRow.AdGroupAd.Ad.ResponsiveSearchAd.Headlines,
                                Descriptions = googleAdsRow.AdGroupAd.Ad.ResponsiveSearchAd.Descriptions,
                                Status = GoogleMapper.AdGroupAdStatusMapperToString(googleAdsRow.AdGroupAd.Status),
                                Type = "Google" 
                            };

                            AdDetails.Add(details);
                        }
                    }
                );
                return new ResponseVM<List<object>>("200", "Successfully fetched all ads", AdDetails);
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }

        }
        #endregion



    }
}
