namespace backend.DTOs.GoogleDtos
{
    public class SearchAdDto
    {
        public List<string> Headlines { get; set; }
        public List<string> Descriptions { get; set; }
        public string TargetUrl { get; set; }
        public string? Path1 { get; set; }
        public string? Path2 { get; set; }
    }
}
