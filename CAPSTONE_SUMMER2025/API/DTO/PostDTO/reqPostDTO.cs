namespace API.DTO.PostDTO
{
    public class ReqPostDTO
    {
        public int? AccountId { get; set; }
        public int? StartupId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<IFormFile>? MediaFiles { get; set; }


    }

    public class ReqPostWithImageDTO
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public IFormFile Image { get; set; }
    }
}
