using backend.Configurations;
using backend.DTOs;
using backend.Service.Interfaces;
using backend.ViewModels;
using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V16.Services;
using Google.Ads.GoogleAds.V16.Errors;
using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.V16.Resources;
using Google.Api.Gax;
using Google.Ads.Gax.Examples;
using static Google.Ads.GoogleAds.V16.Resources.Campaign.Types;
using static Google.Ads.GoogleAds.V16.Enums.CampaignStatusEnum.Types;
using static Google.Ads.GoogleAds.V16.Enums.AdvertisingChannelTypeEnum.Types;
using Google.Ads.GoogleAds.V16.Common;
using static Google.Ads.GoogleAds.V16.Enums.BudgetDeliveryMethodEnum.Types;
using Google.Apis.Auth.OAuth2;
using System.Reflection.Metadata;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using static Google.Rpc.Context.AttributeContext.Types;

namespace backend.Service
{
    public class GoogleApiService : IGoogleApiService
    {
        private readonly HttpClient _httpClient;

        public GoogleApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
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
        public async Task<string> CreateCustomer(long customerId)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = "1//03v7pNMJs1LOPCgYIARAAGAMSNwF-L9IrDpDmkd1-ga1Y6jAaYrYtfqi6Re3xy31rPhoVQvl7OgAuTDgmkdxnsqHV7kCERZ-WuNc",
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

                // Display the result.
                Console.WriteLine($"Created a customer with resource name " +
                    $"'{response.ResourceName}' under the manager account with customer " +
                    $"ID '{Constants.GoogleCustomerId.ToString()}'");
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }
            return "";
        }
        public async Task<string> GetAllCampaigns(long customerId)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = "1//03v7pNMJs1LOPCgYIARAAGAMSNwF-L9IrDpDmkd1-ga1Y6jAaYrYtfqi6Re3xy31rPhoVQvl7OgAuTDgmkdxnsqHV7kCERZ-WuNc",
                LoginCustomerId = Constants.GoogleCustomerId.ToString()
            };

            GoogleAdsClient client = new GoogleAdsClient(config);

            // Get the GoogleAdsService.

            var googleAdsService = client.GetService(Services.V16.GoogleAdsService);

            // Create a query that will retrieve all campaigns.

            string query = @"SELECT
                            campaign.id,
                            campaign.name,
                            campaign.network_settings.target_content_network
                        FROM campaign
                        ORDER BY campaign.id";

            try
            {
                googleAdsService.SearchStream(customerId.ToString(), query,
                    delegate (SearchGoogleAdsStreamResponse resp)
                    {
                        foreach (GoogleAdsRow googleAdsRow in resp.Results)
                        {
                            Console.WriteLine("Campaign with ID {0} and name '{1}' was found.",
                                googleAdsRow.Campaign.Id, googleAdsRow.Campaign.Name);
                        }
                    }
                );
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }
            return "";
        }



        public  async Task<ResponseVM<List<AccountHierarchyDto>>> GetAccountHierarchy(string refreshToken, long? customerId = null)
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
                    catch(Exception ex)
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

        //private async Task<ResponseVM<AccountHierarchyDto>> PrintAccountHierarchy(CustomerClient customerClient,
        //    Dictionary<long, List<CustomerClient>> customerIdsToChildAccounts, int depth)
        //{
        //    long customerId = customerClient.Id;
        //    var manager = new AccountHierarchyDto();
        //    if (depth == 0)
        //    {
        //        manager.CustomerId = customerId;
        //        manager.CurrencyCode = customerClient.CurrencyCode;
        //        manager.DescriptiveName = customerClient.DescriptiveName;
        //        manager.TimeZone = customerClient.TimeZone;

        //    }

        //        Console.WriteLine("Customer ID (Descriptive Name, Currency Code, Time Zone)");


        //    if (customerIdsToChildAccounts.ContainsKey(customerId))
        //        foreach (CustomerClient childAccount in customerIdsToChildAccounts[customerId])
        //        {
        //           manager.ChildAccounts.Add(new AccountHierarchyDto
        //           {
        //               CustomerId = childAccount.Id,
        //               CurrencyCode = childAccount.CurrencyCode,
        //               DescriptiveName = childAccount.DescriptiveName,
        //               TimeZone = childAccount.TimeZone
        //           });
        //        }
        //    return new ResponseVM<AccountHierarchyDto>("200", "Successfully fetched data of accounts", manager);
        //}
        private async Task<AccountHierarchyDto> PrintAccountHierarchy(CustomerClient customerClient,
    Dictionary<long, List<CustomerClient>> customerIdsToChildAccounts, int depth)
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
                        CustomerId = childAccount.Id, CurrencyCode = childAccount.CurrencyCode,
                        DescriptiveName = childAccount.DescriptiveName, TimeZone = childAccount.TimeZone
                    };
                    if (childDto != null)
                    {
                        manager.ChildAccounts.Add(childDto);
                    }
                }
            }

            return manager;
        }


        public async Task<ResponseVM<string>> CreateCampaigns(GoogleCampaignDto campaignDto)
        {
            GoogleAdsConfig config = new GoogleAdsConfig()
            {
                DeveloperToken = Constants.GoogleDeveloperToken,
                OAuth2Mode = Google.Ads.Gax.Config.OAuth2Flow.APPLICATION,
                OAuth2ClientId = Constants.GoogleClientId,
                OAuth2ClientSecret = Constants.GoogleClientSecret,
                OAuth2RefreshToken = campaignDto.RefreshToken,                //"1//03v7pNMJs1LOPCgYIARAAGAMSNwF-L9IrDpDmkd1-ga1Y6jAaYrYtfqi6Re3xy31rPhoVQvl7OgAuTDgmkdxnsqHV7kCERZ-WuNc",
                LoginCustomerId =campaignDto.CustomerId.ToString()           //Constants.GoogleCustomerId.ToString()
            };

            GoogleAdsClient client = new GoogleAdsClient(config);
            CampaignServiceClient campaignService = client.GetService(Services.V16.CampaignService);

            // Create a budget to be used for the campaign.
            string budget = CreateBudget(client, campaignDto.CustomerId);

            List<CampaignOperation> operations = new List<CampaignOperation>();

            Campaign campaign = new Campaign()
            {
                Name = "Interplanetary Cruise #" + ExampleUtilities.GetRandomString(),
                AdvertisingChannelType = AdvertisingChannelType.Search,

                // Recommendation: Set the campaign to PAUSED when creating it to prevent
                // the ads from immediately serving. Set to ENABLED once you've added
                // targeting and the ads are ready to serve
                Status = CampaignStatus.Paused,

                // Set the bidding strategy and budget.
                ManualCpc = new ManualCpc(),
                CampaignBudget = budget,

                // Set the campaign network options.
                NetworkSettings = new NetworkSettings
                {
                    TargetGoogleSearch = true,
                    TargetSearchNetwork = true,
                    // Enable Display Expansion on Search campaigns. See
                    // https://support.google.com/google-ads/answer/7193800 to learn more.
                    TargetContentNetwork = true,
                    TargetPartnerSearchNetwork = false
                },

                // Optional: Set the start date.
                StartDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd"),

                // Optional: Set the end date.
                EndDate = DateTime.Now.AddYears(1).ToString("yyyyMMdd"),
            };

            // Create the operation.
            operations.Add(new CampaignOperation() { Create = campaign });

            try
            {
                // Add the campaigns.
                MutateCampaignsResponse retVal = campaignService.MutateCampaigns(
                    customerId.ToString(), operations);

                // Display the results.
                if (retVal.Results.Count > 0)
                {
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
            catch (GoogleAdsException e)
            {
                return new ResponseVM<string>("400", e.Message);

                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }

        }
        private static string CreateBudget(GoogleAdsClient client, long customerId)
        {
            // Get the BudgetService.
            CampaignBudgetServiceClient budgetService = client.GetService(
                Services.V16.CampaignBudgetService);

            // Create the campaign budget.
            CampaignBudget budget = new CampaignBudget()
            {
                Name = "Interplanetary Cruise Budget #" + ExampleUtilities.GetRandomString(),
                DeliveryMethod = BudgetDeliveryMethod.Standard,
                AmountMicros = 500000
            };

            // Create the operation.
            CampaignBudgetOperation budgetOperation = new CampaignBudgetOperation()
            {
                Create = budget
            };

            // Create the campaign budget.
            MutateCampaignBudgetsResponse response = budgetService.MutateCampaignBudgets(
                customerId.ToString(), new CampaignBudgetOperation[] { budgetOperation });
            return response.Results[0].ResourceName;
        }
    }
}
