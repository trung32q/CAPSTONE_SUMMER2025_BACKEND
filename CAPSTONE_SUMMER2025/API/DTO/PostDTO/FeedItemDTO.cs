namespace API.DTO.PostDTO
{
    public class FeedItemDTO
    {
        public int PostId { get; set; }
        public int? AccountID { get; set; }
        public string AvatarURL { get; set; }
        public string name { get; set; }    
        public string Type { get; set; } // "Post" hoặc "Internship"
        public string Title { get; set; }
        public string Content { get; set; }
        public List<PostMediaDTO>? PostMedia { get; set; }
        public int? StartupId { get; set; }
        public int? LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Priority { get; set; }
        public int InteractionCount { get; set; }
        public int? PostShareId {  get; set; }

        public DateTime? DueDate { get; set; }
        public string? Address {  get; set; }
        
    }
}
