namespace API.DTO.StartupDTO
{
    public class UpdatePermissionDto
    {
        public int PermissionId { get; set; }
        public bool? CanManagePost { get; set; }
        public bool? CanManageCandidate { get; set; }
        public bool? CanManageChatRoom { get; set; }
        public bool? CanManageMember { get; set; }
        public bool? CanManageMilestone { get; set; }
        public bool? CanManageStartupchat{ get; set; }

    }
}
