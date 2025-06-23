namespace API.DTO.PostDTO
{
    public class SharePostRequest
    {
        public int OriginalPostId { get; set; }
        public int? AccountId { get; set; }
        public string? Content { get; set; }
        public string? Title { get; set; }

    }
}
