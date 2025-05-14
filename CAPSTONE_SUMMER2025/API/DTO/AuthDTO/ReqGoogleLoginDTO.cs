using System.ComponentModel.DataAnnotations;

namespace API.DTO.AuthDTO
{
    public class ReqGoogleLoginDTO
    {

        [Required]
        public string GoogleToken { get; set; }
    }
}
