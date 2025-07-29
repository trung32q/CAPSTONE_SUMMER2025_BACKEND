using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO.PostDTO;
using API.DTO.StartupDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace UnitTest.Repositories
{
    public class CVRepositoryTest
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly Mock<IFilebaseHandler> _filebaseHandlerMock;
        private readonly CVRepository _cvRepository; 

        public CVRepositoryTest()
        {
            _context = TestHelper.CreateInMemoryDbContext();
            _filebaseHandlerMock = new Mock<IFilebaseHandler>();
            _cvRepository = new CVRepository(_context, _filebaseHandlerMock.Object);
        }

        #region AddCandidateCvAsync
        [Fact]
        public async Task AddCandidateCvAsync_ValidCv_AddsSuccessfully()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var position = new PositionRequirement
            {
                PositionId = 1,
                Title = "Developer",
                StartupId = 1,
                Description = "Dev role",
                Requirement = "Coding skills"
            };
            var internship = new InternshipPost
            {
                InternshipId = 1,
                StartupId = 1,
                PositionId = 1,
                Status = "Open",
                Description = "Software development internship",
                Requirement = "Basic programming skills",
                Benefits = "Mentorship and learning"
            };
            var account = new Account
            {
                AccountId = 1,
                Email = "test@example.com",
                AccountProfile = new AccountProfile
                {
                    FirstName = "John",
                    LastName = "Doe"
                }
            };
            _context.Startups.Add(startup);
            _context.PositionRequirements.Add(position);
            _context.InternshipPosts.Add(internship);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var cv = new CandidateCv
            {
                AccountId = 1,
                InternshipId = 1,
                Cvurl = "cv.pdf",
                CreateAt = DateTime.UtcNow,
                Status = "Submitted"
            };

            // Act
            var result = await _cvRepository.AddCandidateCvAsync(cv);

            // Assert
            var savedCv = await _context.CandidateCvs.FirstOrDefaultAsync(c => c.CandidateCvId == result);
            Assert.NotNull(savedCv);
            Assert.Equal(1, savedCv.AccountId);
            Assert.Equal(1, savedCv.InternshipId);
            Assert.Equal("cv.pdf", savedCv.Cvurl);
            Assert.Equal("Submitted", savedCv.Status);
            Assert.NotNull(savedCv.CreateAt);
        }
        #endregion

        #region SaveChangesAsync
        [Fact]
        public async Task SaveChangesAsync_WithPendingChanges_SavesSuccessfully()
        {
            // Arrange
            var cv = new CandidateCv
            {
                CandidateCvId = 1,
                Cvurl = "cv.pdf",
                CreateAt = DateTime.UtcNow,
                Status = "Submitted"
            };
            _context.CandidateCvs.Add(cv);

            // Act
            await _cvRepository.SaveChangesAsync();

            // Assert
            var savedCv = await _context.CandidateCvs.FindAsync(1);
            Assert.NotNull(savedCv);
            Assert.Equal("cv.pdf", savedCv.Cvurl);
            Assert.Equal("Submitted", savedCv.Status);
        }
        #endregion

        #region GetCandidateCVsByStartupIdAsync
        [Fact]
        public async Task GetCandidateCVsByStartupIdAsync_ValidStartupId_ReturnsPagedResults()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var position = new PositionRequirement
            {
                PositionId = 1,
                Title = "Developer",
                StartupId = 1,
                Description = "Dev role"
            };
            var internship = new InternshipPost
            {
                InternshipId = 1,
                StartupId = 1,
                PositionId = 1,
                Status = "Open",
                Description = "Software internship",
                Position = position,
                Startup = startup
            };
            var account = new Account
            {
                AccountId = 1,
                Email = "test@example.com",
                AccountProfile = new AccountProfile
                {
                    FirstName = "John",
                    LastName = "Doe",
                    AvatarUrl = "avatar.jpg"
                }
            };
            var cv = new CandidateCv
            {
                CandidateCvId = 1,
                AccountId = 1,
                InternshipId = 1,
                Cvurl = "cv.pdf",
                CreateAt = DateTime.UtcNow,
                Status = "Submitted",
                Account = account,
                Internship = internship
            };
            var evaluation = new CvrequirementEvaluation
            {
                CandidateCvId = 1,
                InternshipId = 1,
                EvaluationTechSkills = "Good",
                EvaluationExperience = "Average",
                EvaluationSoftSkills = "Excellent",
                EvaluationOverallSummary = "Promising candidate"
            };
            _context.Startups.Add(startup);
            _context.PositionRequirements.Add(position);
            _context.InternshipPosts.Add(internship);
            _context.Accounts.Add(account);
            _context.CandidateCvs.Add(cv);
            _context.CvrequirementEvaluations.Add(evaluation);
            await _context.SaveChangesAsync();

            _filebaseHandlerMock.Setup(f => f.GeneratePresignedPDFUrl("cv.pdf", 2))
                .Returns("https://presigned.url/cv.pdf");

            // Act
            var result = await _cvRepository.GetCandidateCVsByStartupIdAsync(1, 0, 1, 10);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            var dto = result.Items[0];
            Assert.Equal(1, dto.CandidateCV_ID);
            Assert.Equal("https://presigned.url/cv.pdf", dto.CVURL);
            Assert.Equal("Submitted", dto.Status);
            Assert.Equal("test@example.com", dto.Email);
            Assert.Equal("John Doe", dto.FullName);
            Assert.Equal("Developer", dto.PositionRequirement);
            Assert.Equal(1, dto.PositionId);
            Assert.Equal("avatar.jpg", dto.AvatarUrl);
            Assert.Equal(1, dto.AccountId);
            Assert.NotNull(dto.CVRequirementEvaluation);
            Assert.Equal("Good", dto.CVRequirementEvaluation.Evaluation_TechSkills);
            Assert.Equal("Promising candidate", dto.CVRequirementEvaluation.Evaluation_OverallSummary);
        }

        [Fact]
        public async Task GetCandidateCVsByStartupIdAsync_NonExistentStartup_ReturnsEmptyList()
        {
            // Arrange
            int startupId = 999;
            int userId = 0;
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var result = await _cvRepository.GetCandidateCVsByStartupIdAsync(startupId, userId, pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.False(result.HasNextPage);
            Assert.False(result.HasPreviousPage);
            Assert.Equal(pageNumber, result.PageNumber);
            Assert.Equal(pageSize, result.PageSize);
        }
        #endregion

        #region GetEvaluationByCandidateIdAsync
        [Fact]
        public async Task GetEvaluationByCandidateIdAsync_ExistingEvaluation_ReturnsDto()
        {
            // Arrange
            var evaluation = new CvrequirementEvaluation
            {
                CandidateCvId = 1,
                InternshipId = 1,
                EvaluationTechSkills = "Good",
                EvaluationExperience = "Average",
                EvaluationSoftSkills = "Excellent",
                EvaluationOverallSummary = "Promising candidate"
            };
            _context.CvrequirementEvaluations.Add(evaluation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cvRepository.GetEvaluationByCandidateIdAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Good", result.Evaluation_TechSkills);
            Assert.Equal("Average", result.Evaluation_Experience);
            Assert.Equal("Excellent", result.Evaluation_SoftSkills);
            Assert.Equal("Promising candidate", result.Evaluation_OverallSummary);
        }
        #endregion

        #region GetCandidateCvWithRelationsAsync
        [Fact]
        public async Task GetCandidateCvWithRelationsAsync_ValidCvId_ReturnsCvWithRelations()
        {
            // Arrange
            var startup = new Startup { StartupId = 1, StartupName = "Test Startup" };
            var position = new PositionRequirement
            {
                PositionId = 1,
                Title = "Developer",
                StartupId = 1,
                Description = "Dev role"
            };
            var internship = new InternshipPost
            {
                InternshipId = 1,
                StartupId = 1,
                PositionId = 1,
                Status = "Open",
                Description = "Internship",
                Startup = startup,
                Position = position
            };
            var account = new Account
            {
                AccountId = 1,
                Email = "test@example.com",
                AccountProfile = new AccountProfile
                {
                    FirstName = "John",
                    LastName = "Doe",
                    AvatarUrl = "avatar.jpg"
                }
            };
            var cv = new CandidateCv
            {
                CandidateCvId = 1,
                AccountId = 1,
                InternshipId = 1,
                Cvurl = "cv.pdf",
                CreateAt = DateTime.UtcNow,
                Status = "Submitted"
            };
            _context.Startups.Add(startup);
            _context.PositionRequirements.Add(position);
            _context.InternshipPosts.Add(internship);
            _context.Accounts.Add(account);
            _context.CandidateCvs.Add(cv);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cvRepository.GetCandidateCvWithRelationsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CandidateCvId);
            Assert.NotNull(result.Account);
            Assert.Equal("test@example.com", result.Account.Email);
            Assert.NotNull(result.Account.AccountProfile);
            Assert.Equal("John", result.Account.AccountProfile.FirstName);
            Assert.Equal("avatar.jpg", result.Account.AccountProfile.AvatarUrl);
            Assert.NotNull(result.Internship);
            Assert.Equal("Internship", result.Internship.Description);
            Assert.NotNull(result.Internship.Position);
            Assert.Equal("Developer", result.Internship.Position.Title);
            Assert.NotNull(result.Internship.Startup);
            Assert.Equal("Test Startup", result.Internship.Startup.StartupName);
        }

        [Fact]
        public async Task GetCandidateCvWithRelationsAsync_NonExistentCvId_ReturnsNull()
        {
            // Act
            var result = await _cvRepository.GetCandidateCvWithRelationsAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetCandidateCVByIdAsync
        [Fact]
        public async Task GetCandidateCVByIdAsync_ValidCvId_ReturnsCv()
        {
            // Arrange
            var cv = new CandidateCv
            {
                CandidateCvId = 1,
                Cvurl = "cv.pdf",
                CreateAt = DateTime.UtcNow,
                Status = "Submitted"
            };
            _context.CandidateCvs.Add(cv);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cvRepository.GetCandidateCVByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CandidateCvId);
            Assert.Equal("cv.pdf", result.Cvurl);
            Assert.Equal("Submitted", result.Status);
            Assert.NotNull(result.CreateAt);
        }

        [Fact]
        public async Task GetCandidateCVByIdAsync_NonExistentCvId_ReturnsNull()
        {
            // Act
            var result = await _cvRepository.GetCandidateCVByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region HasSubmittedCVAsync
        [Fact]
        public async Task HasSubmittedCVAsync_ExistingSubmission_ReturnsTrue()
        {
            // Arrange
            var cv = new CandidateCv
            {
                CandidateCvId = 1,
                AccountId = 1,
                InternshipId = 1,
                Cvurl = "cv.pdf",
                CreateAt = DateTime.UtcNow,
                Status = "Submitted"
            };
            _context.CandidateCvs.Add(cv);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cvRepository.HasSubmittedCVAsync(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasSubmittedCVAsync_NonExistentSubmission_ReturnsFalse()
        {
            // Act
            var result = await _cvRepository.HasSubmittedCVAsync(999, 999);

            // Assert
            Assert.False(result);
        }
        #endregion
    }
}