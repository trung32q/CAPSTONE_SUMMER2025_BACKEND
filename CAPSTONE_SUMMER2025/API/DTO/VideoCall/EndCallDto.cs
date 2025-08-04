namespace API.DTO.VideoCall
{
    public class EndCallDto
    {
        public string RoomToken { get; set; }
        public Guid CallSessionId { get; set; }
    }
}
