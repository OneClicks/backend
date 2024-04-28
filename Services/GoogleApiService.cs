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

namespace backend.Service
{
    public class GoogleApiService : IGoogleApiService
    {
        private readonly HttpClient _httpClient;

        public GoogleApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<string> GetAllCampaigns(long customerId, string refre)
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
        public string GetAccountHierarchy(long? customerId = null)
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
                    PagedEnumerable<SearchGoogleAdsResponse, GoogleAdsRow> response =
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

                if (rootCustomerClient != null)
                {
                    Console.WriteLine("The hierarchy of customer ID {0} is printed below:",
                        rootCustomerClient.Id);
                    PrintAccountHierarchy(rootCustomerClient, customerIdsToChildAccounts, 0);
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine(
                        "Customer ID {0} is likely a test account, so its customer client " +
                        " information cannot be retrieved.", managerCustomerId);
                }
            }

            return "";
        }

        private const int PAGE_SIZE = 1000;

        private void PrintAccountHierarchy(CustomerClient customerClient,
            Dictionary<long, List<CustomerClient>> customerIdsToChildAccounts, int depth)
        {
            if (depth == 0)
                Console.WriteLine("Customer ID (Descriptive Name, Currency Code, Time Zone)");

            long customerId = customerClient.Id;
            Console.Write(new string('-', depth * 2));
            Console.WriteLine("{0} ({1}, {2}, {3})",
                customerId, customerClient.DescriptiveName, customerClient.CurrencyCode,
                customerClient.TimeZone);

            if (customerIdsToChildAccounts.ContainsKey(customerId))
                foreach (CustomerClient childAccount in customerIdsToChildAccounts[customerId])
                    PrintAccountHierarchy(childAccount, customerIdsToChildAccounts,
                        depth + 1);
        }

    }
}
