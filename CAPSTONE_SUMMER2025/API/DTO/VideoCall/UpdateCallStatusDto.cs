namespace API.DTO.VideoCall
{
    public class UpdateCallStatusDto
    {
        public string RoomToken { get; set; }
        public Guid CallSessionId { get; set; }
    }
}
