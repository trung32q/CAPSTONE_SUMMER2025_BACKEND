using System.ComponentModel.DataAnnotations;

namespace API.DTO.StartupDTO
{
    public class StartupPitchingUpdateDTO
    {
        public int PitchingId { get; set; }

        public IFormFile File
        {
            get; set;
        }
    }
}
