namespace backend.DTOs
{
    public class AdCreativeDto
    {
        public string CreativeId { get; set; }
        public string CreativeName { get; set; }
        public string PageId { get; set; }
        public string AdsetId { get; set; }
        public string FileName { get; set; }
        public string ImageHash { get; set; }
        public string Message { get; set; }
        public string AdAccountId { get; set; }
        public string AccessToken { get; set; }
        public string Type { get; set; }
    }
    public class AdImageDto
    {
        public string AdAccountId { get; set; }
        public IFormFile ImageFile { get; set; }
        public string AccessToken { get; set; }

    }

    public class ImageHashDto
    {
        public string Hash { get; set; }

    }

}
