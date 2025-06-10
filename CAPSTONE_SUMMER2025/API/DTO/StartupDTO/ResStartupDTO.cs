namespace API.DTO.StartupDTO
{
    public class ResStartupDTO
    {
        public int Startup_ID { get; set; }
        public string Startup_Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public string BackgroundUrl { get; set; }        
        public string WebsiteURL { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public List<string> Categories { get; set; }
    }
}
