using System.Security.Cryptography;
using System.Text;
using static Google.Ads.GoogleAds.V16.Enums.AdvertisingChannelTypeEnum.Types;
using static Google.Ads.GoogleAds.V16.Enums.BudgetDeliveryMethodEnum.Types;
using static Google.Ads.GoogleAds.V16.Enums.CampaignStatusEnum.Types;

namespace backend.Helpers
{
    public static class HelperFunctions
    {
        public static string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        }
        public static string CreateRandomPassword(int length = 8)
        {

            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+";

            if (length <= 0)
            {
                throw new ArgumentException("Password length must be greater than 0.");
            }

            StringBuilder password = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(allowedChars.Length);
                password.Append(allowedChars[index]);
            }

            return password.ToString();


        }
        public static class GuidGenerator
        {
            public static readonly Guid PostFixSecretKey = Guid.NewGuid();
        }
    }
    public static class GoogleMapper
    {
        public static AdvertisingChannelType MapToEnum(string advertisingChannelType)
        {
            return advertisingChannelType.ToUpper() switch
            {
                "DISCOVERY" => AdvertisingChannelType.Discovery,
                "DISPLAY" => AdvertisingChannelType.Display,
                "HOTEL" => AdvertisingChannelType.Hotel,
                "LOCAL" => AdvertisingChannelType.Local,
                "LOCAL_SERVICES" => AdvertisingChannelType.LocalServices,
                "MULTI_CHANNEL" => AdvertisingChannelType.MultiChannel,
                "PERFORMANCE_MAX" => AdvertisingChannelType.PerformanceMax,
                "SEARCH" => AdvertisingChannelType.Search,
                "SHOPPING" => AdvertisingChannelType.Shopping,
                "SMART" => AdvertisingChannelType.Smart,
                "TRAVEL" => AdvertisingChannelType.Travel,
                "UNKNOWN" => AdvertisingChannelType.Unknown,
                "UNSPECIFIED" => AdvertisingChannelType.Unspecified,
                "VIDEO" => AdvertisingChannelType.Video,
                _ => throw new ArgumentException($"Invalid advertising channel type: {advertisingChannelType}")
            };

        }
        public static CampaignStatus CampaignStatusMapper(string status)
        {
            return status.ToUpper() switch
            {
                "PAUSED" => CampaignStatus.Paused,
                "ENABLED" => CampaignStatus.Enabled,
                "REMOVED" => CampaignStatus.Removed,
                "UNKNOWN" => CampaignStatus.Unknown,
                "UNSPECIFIED" => CampaignStatus.Unspecified,
                _ => throw new ArgumentException($"Invalid campaign status: {status}")
            };
        }
        public static BudgetDeliveryMethod BudgetDeliveryMethodMapper(string deliveryMethod)
        {
            return deliveryMethod.ToUpper() switch
            {
                "ACCELERATED" => BudgetDeliveryMethod.Accelerated,
                "STANDARD" => BudgetDeliveryMethod.Standard,
                "UNKNOWN" => BudgetDeliveryMethod.Unknown,
                "UNSPECIFIED" => BudgetDeliveryMethod.Unspecified,
                _ => throw new ArgumentException($"Invalid budget delivery method: {deliveryMethod}")
            };
        }
    }
    

}
