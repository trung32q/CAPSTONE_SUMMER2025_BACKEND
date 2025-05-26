namespace API.Repositories.Interfaces
{
    public interface IFptAIRepository
    {
        Task<bool> VerifyFaceAsync(IFormFile cccdImage, IFormFile selfieImage);
    }
}
