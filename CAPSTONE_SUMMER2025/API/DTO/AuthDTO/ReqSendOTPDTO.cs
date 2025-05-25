using System.ComponentModel.DataAnnotations;

namespace API.DTO.AuthDTO
{
    public class ReqSendOTPDTO
    {

        [Required]
        public string Email { get; set; }
    }
}
