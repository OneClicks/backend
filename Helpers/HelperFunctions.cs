using Google.Ads.GoogleAds.V16.Resources;
using System.Security.Cryptography;
using System.Text;
using static Google.Ads.GoogleAds.V16.Enums.AdGroupAdStatusEnum.Types;
using static Google.Ads.GoogleAds.V16.Enums.AdGroupCriterionStatusEnum.Types;
using static Google.Ads.GoogleAds.V16.Enums.AdGroupStatusEnum.Types;
using static Google.Ads.GoogleAds.V16.Enums.AdvertisingChannelTypeEnum.Types;
using static Google.Ads.GoogleAds.V16.Enums.BudgetDeliveryMethodEnum.Types;
using static Google.Ads.GoogleAds.V16.Enums.CampaignStatusEnum.Types;
using static Google.Ads.GoogleAds.V16.Enums.KeywordMatchTypeEnum.Types;

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

        public static string CampaignStatusToString(CampaignStatus status)
        {
            return status switch
            {
                CampaignStatus.Paused => "PAUSED",
                CampaignStatus.Enabled => "ENABLED",
                CampaignStatus.Removed => "REMOVED",
                CampaignStatus.Unknown => "UNKNOWN",
                CampaignStatus.Unspecified => "UNSPECIFIED",
                _ => throw new ArgumentException($"Invalid campaign status: {status}")
            };
        }

        public static AdGroupStatus AdGroupStatusMapper(string status)
        {
            return status.ToUpper() switch
            {
                "PAUSED" => AdGroupStatus.Paused,
                "ENABLED" => AdGroupStatus.Enabled,
                "REMOVED" => AdGroupStatus.Removed,
                "UNKNOWN" => AdGroupStatus.Unknown,
                "UNSPECIFIED" => AdGroupStatus.Unspecified,
                _ => throw new ArgumentException($"Invalid AdGroupStatus : {status}")
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
        public static string BudgetDeliveryMethodToString(BudgetDeliveryMethod deliveryMethod)
        {
            return deliveryMethod switch
            {
                BudgetDeliveryMethod.Accelerated => "ACCELERATED",
                BudgetDeliveryMethod.Standard => "STANDARD",
                BudgetDeliveryMethod.Unknown => "UNKNOWN",
                BudgetDeliveryMethod.Unspecified => "UNSPECIFIED",
                _ => throw new ArgumentException($"Invalid budget delivery method: {deliveryMethod}")
            };
        }

        public static AdGroupAdStatus AdGroupAdStatusMapper(string status)
        {
            return status.ToUpper() switch
            {
                "ENABLED" => AdGroupAdStatus.Enabled,
                "PAUSED" => AdGroupAdStatus.Paused,
                "REMOVED" => AdGroupAdStatus.Removed,
                "UNKNOWN" => AdGroupAdStatus.Unknown,
                "UNSPECIFIED" => AdGroupAdStatus.Unspecified,
                _ => throw new ArgumentException($"Invalid AdGroupAdStatus: {status}")
            };
        }
        public static AdGroupCriterionStatus AdGroupCriterionStatusMapper(string status)
        {
            return status.ToUpper() switch
            {
                "ENABLED" => AdGroupCriterionStatus.Enabled,
                "PAUSED" => AdGroupCriterionStatus.Paused,
                "REMOVED" => AdGroupCriterionStatus.Removed,
                "UNKNOWN" => AdGroupCriterionStatus.Unknown,
                "UNSPECIFIED" => AdGroupCriterionStatus.Unspecified,
                _ => throw new ArgumentException($"Invalid AdGroupCriterionStatus: {status}")
            };
        }
        public static KeywordMatchType KeywordMatchTypeMapper(string matchType)
        {
            return matchType.ToUpper() switch
            {
                "EXACT" => KeywordMatchType.Exact,
                "PHRASE" => KeywordMatchType.Phrase,
                "BROAD" => KeywordMatchType.Broad,
                "UNKNOWN" => KeywordMatchType.Unknown,
                "UNSPECIFIED" => KeywordMatchType.Unspecified,
                _ => throw new ArgumentException($"Invalid KeywordMatchType: {matchType}")
            };
        }
    }


}
