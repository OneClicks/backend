using MongoDB.Bson.Serialization.Attributes;
using Org.BouncyCastle.Crmf;

namespace backend.DTOs
{
    public class AdaccountsDto
    {
        public string UserId { get; set; }

        public string AccountId { get; set; }

        public string AdAccountId { get; set; }

        public List<string> Pages { get; set; }

        public string Platform { get; set; }
        public string LongLiveToken { get; set; }
    }

    public class Interest
    {
        public long id { get; set; }
        public string name { get; set; }
    }

    public class Targeting
    {
        public GeoLocations geo_locations { get; set; }
        public List<Interest> interests { get; set; }
        public List<Interest> industries { get; set; }
    }
    public class Cities
    {
        public string key { get; set; }
    }

    public class GeoLocations
    {
        public List<Cities> cities { get; set; }
    }
    public class LocationData
    {
        public string Key { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
    }
    public class AdTargetingCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }


        // Add other properties as needed
    }

}
