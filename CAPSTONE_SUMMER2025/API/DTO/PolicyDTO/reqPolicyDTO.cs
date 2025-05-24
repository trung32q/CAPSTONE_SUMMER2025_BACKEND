namespace API.DTO.PolicyDTO
{
    public class reqPolicyDTO
    {
        public string Description { get; set; }
        public int PolicyTypeId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
