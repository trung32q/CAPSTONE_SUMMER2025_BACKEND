namespace API.DTO.PostDTO
{
    public class resPostDTO
    {
        public int PostId { get; set; }
        public int? AccountId { get; set; }
        public string? Content { get; set; }
        public List<PostMediaDTO> PostMedia { get; set; }
        public string? Title { get; set; }
        public DateTime? CreateAt { get; set; }
        public int LikeCount { get; set; }
        public int? PostShareId { get; set; }
        public DateTime? Schedule { get; set; }
        public string? FullName {  get; set; }
        public string? AvatarUrl {  get; set; }

        public int StartupId {  get; set; }

    }
}
