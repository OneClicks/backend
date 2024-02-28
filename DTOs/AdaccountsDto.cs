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
}
