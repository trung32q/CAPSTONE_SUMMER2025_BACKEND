using API.DTO.AccountDTO;
using API.DTO.PostDTO;
using API.DTO.StartupDTO;
using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class CVRepository : ICVRepository
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly IFilebaseHandler _filebaseHandler;

        public CVRepository(CAPSTONE_SUMMER2025Context context, IFilebaseHandler filebaseHandler)
        {
            _context = context;
            _filebaseHandler = filebaseHandler;
        }

        // apply cv
        public async Task<int> AddCandidateCvAsync(CandidateCv cv)
        {
            await _context.CandidateCvs.AddAsync(cv);
            await _context.SaveChangesAsync();
            return cv.CandidateCvId;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // lấy ra các candidate cv theo statup
        public async Task<PagedResult<CandidateCVResponseDTO>> GetCandidateCVsByStartupIdAsync(
     int startupId, int positionId, int pageNumber, int pageSize)
        {
            var query = _context.CandidateCvs
                .Where(cv => cv.Internship.StartupId == startupId &&
                    (positionId == 0 || cv.Internship.Position.PositionId == positionId))
                .Include(cv => cv.Account)
                    .ThenInclude(a => a.AccountProfile)
                .Include(cv => cv.Internship)
                    .ThenInclude(ip => ip.Position);

            var totalCount = await query.CountAsync();

            var candidateEntities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<CandidateCVResponseDTO>();

            foreach (var cv in candidateEntities)
            {
                var evaluation = await GetEvaluationByCandidateIdAsync(cv.CandidateCvId, (int)cv.InternshipId);

                result.Add(new CandidateCVResponseDTO
                {
                    CandidateCV_ID = cv.CandidateCvId,
                    CVURL = _filebaseHandler.GeneratePresignedPDFUrl(cv.Cvurl, 2),
                    CreateAt = (DateTime)cv.CreateAt,
                    Status = cv.Status,
                    Email = cv.Account.Email,
                    FullName = $"{cv.Account.AccountProfile.FirstName} {cv.Account.AccountProfile.LastName}",
                    PositionRequirement = cv.Internship.Position.Title,
                    PositionId = cv.Internship.Position.PositionId,
                    CVRequirementEvaluation = evaluation,
                    AvatarUrl = cv.Account.AccountProfile.AvatarUrl,
                    AccountId = cv.Account.AccountId
                });
            }

            return new PagedResult<CandidateCVResponseDTO>(result, totalCount, pageNumber, pageSize);
        }

        public async Task<CVRequirementEvaluationResultDto> GetEvaluationByCandidateIdAsync(int candidateCvId, int internshipId)
        {
            var e = await _context.CvrequirementEvaluations
                .FirstOrDefaultAsync(e => e.CandidateCvId == candidateCvId && e.InternshipId == internshipId);
            return new CVRequirementEvaluationResultDto
            {
                Evaluation_Experience = e.EvaluationExperience,
                Evaluation_SoftSkills = e.EvaluationSoftSkills,
                Evaluation_TechSkills = e.EvaluationTechSkills,
                Evaluation_OverallSummary = e.EvaluationOverallSummary
            };
        }

        public async Task<CandidateCv?> GetCandidateCvWithRelationsAsync(int candidateCvId)
        {
            return await _context.CandidateCvs
                .Include(cv => cv.Account)
                    .ThenInclude(a => a.AccountProfile)
                .Include(cv => cv.Internship)
                    .ThenInclude(ip => ip.Position)
                .Include(cv => cv.Internship)
                    .ThenInclude(ip => ip.Startup)
                .FirstOrDefaultAsync(cv => cv.CandidateCvId == candidateCvId);
        }

        public async Task<CandidateCv?> GetCandidateCVByIdAsync(int id)
        {
            return await _context.CandidateCvs.FindAsync(id);
        }

        public async Task<bool> HasSubmittedCVAsync(int accountId, int internshipId)
        {
            return await _context.CandidateCvs
                .AnyAsync(cv => cv.AccountId == accountId && cv.InternshipId == internshipId);
        }

    }
}
