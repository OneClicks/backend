namespace backend.DTOs
{
    public class AccountHierarchyDto
    {
        public long CustomerId { get; set; }
        public string DescriptiveName { get; set; }
        public string CurrencyCode { get; set; }
        public string TimeZone { get; set; }
        public List<AccountHierarchyDto> ChildAccounts { get; set; }
    }

}
