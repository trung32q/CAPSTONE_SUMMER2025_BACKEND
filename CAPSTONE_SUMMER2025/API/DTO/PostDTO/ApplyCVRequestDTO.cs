namespace API.DTO.PostDTO
{
    public class ApplyCVRequestDTO
    {
        public int Account_ID { get; set; }
        public int Internship_ID { get; set; }
        public IFormFile CVFile { get; set; }

        public int PositionId {  get; set; }

    }
}
