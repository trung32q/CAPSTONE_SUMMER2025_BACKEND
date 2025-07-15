using API.DTO.StartupDTO;

namespace API.Service.Interface
{
    public interface ISwotService
    {
        Task<SwotAnalysisDto> AnalyzeSwotAsync(SwotBmcDto dto);
    }
}
