using API.DTO.AuthDTO;
using Infrastructure.Models;

namespace API.Service.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlContent);
       

    }
}
