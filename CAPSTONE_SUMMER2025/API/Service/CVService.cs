using API.DTO.AccountDTO;
using API.DTO.PostDTO;
using API.DTO.StartupDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service.Interface;
using Infrastructure.Models;

namespace API.Service
{
    public class CVService : ICVService
    {

        private readonly IStartupService _startupService;
        private readonly ICVRepository _cvRepository;
        private readonly IFilebaseHandler _filebaseHandler;
        private readonly IFileHandlerService _filehandlerService;
        private readonly IChatGPTRepository _chatGPTRepository;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public CVService(IChatGPTRepository chatGPTRepository,IConfiguration config, IEmailService emailService, IStartupService startService, ICVRepository cvRepository, IFilebaseHandler filebaseHandler, IFileHandlerService filehandlerService)
        {
            _config = config;
            _emailService = emailService;
            _startupService = startService;
            _cvRepository = cvRepository;
            _filebaseHandler = filebaseHandler;
            _filehandlerService = filehandlerService;
            _chatGPTRepository = chatGPTRepository;
        }

        public async Task<bool> ApplyCVAsync(ApplyCVRequestDTO dto)
        {

            var positionInfor = await _startupService.GetRequirementInfoAsync(dto.PositionId);

            var cvURL = await _filebaseHandler.UploadPdfAsync((IFormFile)dto.CVFile);
            var cvText = await _filehandlerService.GetTextFromPdfAsync(dto.CVFile);
            var evaluationCV = await _chatGPTRepository.EvaluateCVAgainstPositionAsync(cvText, positionInfor.Description, positionInfor.Requirement);



            var cv = new CandidateCv
            {
                AccountId = dto.Account_ID,
                InternshipId = dto.Internship_ID,
                Cvurl = cvURL,
                CreateAt = DateTime.Now,
                Status = Utils.Constants.CVStatus.PENDING
            };

            var candidateId = await _cvRepository.AddCandidateCvAsync(cv);

            await _startupService.AddEvaluationAsync(new DTO.StartupDTO.CVRequirementEvaluationResultDto
            {
                CandidateCVID = candidateId,
                InternshipId = dto.Internship_ID,
                Evaluation_Experience = evaluationCV.Evaluation_Experience,
                Evaluation_SoftSkills = evaluationCV.Evaluation_SoftSkills,
                Evaluation_TechSkills = evaluationCV.Evaluation_TechSkills,
                Evaluation_OverallSummary = evaluationCV.Evaluation_OverallSummary
            });

            await _cvRepository.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResult<CandidateCVResponseDTO>> GetCVsOfStartupAsync(int startupId, int positionId, int page, int pageSize)
        {
            return await _cvRepository.GetCandidateCVsByStartupIdAsync(startupId, positionId, page, pageSize);
        }

        public async Task<bool> ResponseCandidateCVAsync(int candidateCVId, string newStatus)
        {
            var cv = await _cvRepository.GetCandidateCVByIdAsync(candidateCVId);
            var infor = await GetCandidateInfoAsync(candidateCVId);
            if (cv == null) return false;

            string subject = "";
            string htmlBody = "";

            if (newStatus == Utils.Constants.CVStatus.ACCEPT)
            {
                subject = _config["EmailTemplates:AcceptSubject"];
                htmlBody = _config["EmailTemplates:AcceptHtmlBody"]
                    .Replace("{CandidateFullName}", infor.FullName)
                    .Replace("{StartupName}", infor.StartupName)
                    .Replace("{PositionTitle}", infor.PositionTitle);
            }
            else if (newStatus == Utils.Constants.CVStatus.REJECT)
            {
                subject = _config["EmailTemplates:RejectSubject"];
                htmlBody = _config["EmailTemplates:RejectHtmlBody"]
                    .Replace("{CandidateFullName}", infor.FullName)
                    .Replace("{StartupName}", infor.StartupName)
                    .Replace("{PositionTitle}", infor.PositionTitle);
            }

            await _emailService.SendEmailAsync(infor.email, subject, htmlBody);

            cv.Status = newStatus;
            await _cvRepository.SaveChangesAsync();
            return true;
        }

        public async Task<CandidateInfoDto?> GetCandidateInfoAsync(int candidateCvId)
        {
            var cv = await _cvRepository.GetCandidateCvWithRelationsAsync(candidateCvId);
            if (cv == null) return null;

            return new CandidateInfoDto
            {
                FullName = $"{cv.Account?.AccountProfile?.FirstName} {cv.Account?.AccountProfile?.LastName}",
                PositionTitle = cv.Internship?.Position?.Title ?? "",
                StartupName = cv.Internship?.Startup?.StartupName ?? "",
                email = cv.Account?.Email

            };
        }

        public async Task<bool> CheckSubmittedCVAsync(int accountId, int internshipId)
        {
            return await _cvRepository.HasSubmittedCVAsync(accountId, internshipId);
        }

    }
}
