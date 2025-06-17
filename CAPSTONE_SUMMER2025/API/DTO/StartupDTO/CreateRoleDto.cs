namespace API.DTO.StartupDTO
{
    public class CreateRoleDto
    {
        public int Startup_ID { get; set; }
        public string RoleName { get; set; }
    }

    public class UpdateRoleDto
    {
        public int Role_ID { get; set; }
        public string RoleName { get; set; }
    }
}
