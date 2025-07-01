namespace API.DTO.PostDTO
{
    public class PostSearchDTO
    {
        public int PostID { get; set; }
        public string? Title { get; set; }
        public DateTime CreateAt { get; set; }
        public string? Content { get; set; }
        public string? StartupName { get; set; }
        public string? StartupLogo { get; set; }
        public List<PostMediaDTO> Media { get; set; } = new List<PostMediaDTO>();
    }
}
