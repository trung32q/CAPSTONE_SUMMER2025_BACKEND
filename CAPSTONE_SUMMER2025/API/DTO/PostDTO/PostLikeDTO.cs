namespace API.DTO.PostDTO
{
    public class PostLikeDTO
    {
        public int PostLikeId { get; set; }
        public int? PostId { get; set; }
        public int? AccountId { get; set; }
        public DateTime? LikedAt { get; set; }
        public string AvatarUrl { get; set; }
        public string FullName { get; set; }
    }
}
