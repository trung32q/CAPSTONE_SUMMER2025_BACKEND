namespace API.DTO.PostDTO
{
    public class CreatePostReportDTO
    {
        public int AccountId { get; set; }
        public int PostId { get; set; }
        public int ReasonId { get; set; }
    }

    public class PostReportDTO
    {
        public int ReportId { get; set; }
        public int AccountId { get; set; }
        public int PostId { get; set; }
        public int ReasonId { get; set; }
        public string Status { get; set; }
        public DateTime CreateAt { get; set; }
    }

}
