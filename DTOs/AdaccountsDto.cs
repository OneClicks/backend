using MongoDB.Bson.Serialization.Attributes;

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
    }

    public class GeoLocations
    {
        public List<string> countries { get; set; }
    }

}
