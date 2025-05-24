namespace API.DTO.PolicyDTO
{
    public class resPolicyDTO
    {
        public int PolicyId { get; set; }
        public string Description { get; set; }
        public int PolicyTypeId { get; set; }
        public DateTime CreateAt { get; set; }
        public bool IsActive { get; set; }
    }
}
