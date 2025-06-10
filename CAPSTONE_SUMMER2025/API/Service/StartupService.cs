using API.DTO.StartupDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service.Interface;
using AutoMapper;
using Infrastructure.Models;
using Org.BouncyCastle.Ocsp;

namespace API.Service
{
    public class StartupService : IStartupService
    {
        private readonly IStartupRepository _repo;
        private readonly IMapper _mapper;
        private readonly IFilebaseHandler _filebaseHandler;
        public StartupService(IStartupRepository repo, IMapper mapper, IFilebaseHandler filebaseHandler)
        {
            _repo = repo;
            _mapper = mapper;
            _filebaseHandler = filebaseHandler;
        }
        public async Task<int> CreateStartupAsync(CreateStartupRequest request)
        {
            string logoUrl = null;
            string backgroundUrl = null;

            // Upload logo 
            if (request.Logo != null)
                logoUrl = await _filebaseHandler.UploadMediaFile(request.Logo);

            // Upload background
            if (request.BackgroundUrl != null)
                backgroundUrl = await _filebaseHandler.UploadMediaFile(request.BackgroundUrl);
            // 1. Tạo Startup
            var startup = _mapper.Map<Startup>(request);
            await _repo.AddStartupAsync(startup);

            // 2. Tạo role Founder
            var founderRole = new RoleInStartup
            {
                RoleName = "Founder",
                StartupId = startup.StartupId
            };
            await _repo.AddRoleAsync(founderRole);

            // 3. Gán creator vào làm Founder
            var founderMember = new StartupMember
            {
                StartupId = startup.StartupId,
                AccountId = request.CreatorAccountId,
                RoleId = founderRole.RoleId,
                JoinedAt = DateTime.UtcNow
            };
            await _repo.AddMemberAsync(founderMember);

            // 4. Invite member (nếu có)
            if (request.InviteAccountIds != null && request.InviteAccountIds.Any())
            {
                var memberRole = new RoleInStartup
                {
                    RoleName = "Member",
                    StartupId = startup.StartupId
                };
                await _repo.AddRoleAsync(memberRole);

                foreach (var accId in request.InviteAccountIds.Distinct())
                {
                    if (accId == request.CreatorAccountId) continue;
                    var member = new StartupMember
                    {
                        StartupId = startup.StartupId,
                        AccountId = accId,
                        RoleId = memberRole.RoleId,
                        JoinedAt = DateTime.UtcNow
                    };
                    await _repo.AddMemberAsync(member);
                }
            }

            // 5. Gán Category cho Startup
            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                foreach (var catId in request.CategoryIds.Distinct())
                {
                    var sc = new StartupCategory
                    {
                        StartupId = startup.StartupId,
                        CategoryId = catId
                    };
                    await _repo.AddStartupCategoryAsync(sc);
                }
            }

            return startup.StartupId;
        }
        public async Task<List<ResStartupDTO>> GetAllStartupsAsync()
        {
            var startups = await _repo.GetAllStartupsAsync();
            var result = startups.Select(s => new ResStartupDTO
            {
                Startup_ID = s.StartupId,
                Startup_Name = s.StartupName,
                Description = s.Description,
                // Nếu Logo không rỗng, tạo PreSignedUrl
                Logo = !string.IsNullOrEmpty(s.Logo) ? _filebaseHandler.GeneratePreSignedUrl(
                    s.Logo.Contains("/") ? s.Logo : $"image/{s.Logo}"
                ) : null,
                BackgroundUrl = !string.IsNullOrEmpty(s.BackgroundUrl) ? _filebaseHandler.GeneratePreSignedUrl(
                     s.BackgroundUrl.Contains("/") ? s.BackgroundUrl : $"image/{s.BackgroundUrl}"
                ) : null,

                WebsiteURL = s.WebsiteUrl,
                Email = s.Email,
                Status = s.Status,
                Categories = s.StartupCategories?
                    .Select(sc => sc.Category.CategoryName).ToList() ?? new List<string>()
            }).ToList();

            return result;
        }

    }
}
