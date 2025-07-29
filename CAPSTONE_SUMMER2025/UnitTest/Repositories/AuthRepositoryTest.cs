using API.DTO.AccountDTO;
using API.DTO.AuthDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using UnitTest; // Sử dụng namespace của TestHelper
using Xunit;
using System;

namespace UnitTest.Repositories
{
    public class AuthRepositoryTest
    {
        private readonly IMapper _mapper;
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly IAuthRepository _authRepository;
        private readonly Mock<Microsoft.Extensions.Configuration.IConfiguration> _configurationMock;

        public AuthRepositoryTest()
        {
            _mapper = TestHelper.CreateMapper();
            _context = TestHelper.CreateInMemoryDbContext();
            _configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            _authRepository = new AuthRepository(_mapper, _context, _configurationMock.Object);
        }

        #region CheckEmailExistsAsync
        [Fact]
        public async Task CheckEmailExistsAsync_ExistingEmail_ReturnsTrue()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authRepository.CheckEmailExistsAsync("test@example.com");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckEmailExistsAsync_NonExistingEmail_ReturnsFalse()
        {
            // Arrange

            // Act
            var result = await _authRepository.CheckEmailExistsAsync("nonexistent@example.com");

            // Assert
            Assert.False(result);
        }
        #endregion

        #region AddAccountAsync
        [Fact]
        public async Task AddAccountAsync_ValidAccount_SavesSuccessfully()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");

            // Act
            await _authRepository.AddAccountAsync(account);
            await _authRepository.SaveChangesAsync();

            // Assert
            var result = await _context.Accounts.FindAsync(1);
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
        }
        #endregion

        #region SaveOtpAsync
        [Fact]
        public async Task SaveOtpAsync_ValidOtp_SavesSuccessfully()
        {
            // Arrange
            var otp = new UserOtp { AccountId = 1, OtpCode = "123456", ExpiresAt = DateTime.UtcNow.AddMinutes(5) };

            // Act
            await _authRepository.SaveOtpAsync(otp);
            await _authRepository.SaveChangesAsync();

            // Assert
            var result = await _context.UserOtps.FirstOrDefaultAsync(o => o.AccountId == 1);
            Assert.NotNull(result);
            Assert.Equal("123456", result.OtpCode);
        }
        #endregion

        #region GetAccountByEmailAsync
        [Fact]
        public async Task GetAccountByEmailAsync_ExistingEmail_ReturnsAccount()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authRepository.GetAccountByEmailAsync("test@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AccountProfile);
            Assert.Equal(1, result.AccountId);
        }

        [Fact]
        public async Task GetAccountByEmailAsync_NonExistingEmail_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _authRepository.GetAccountByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAccountByIdAsync
        [Fact]
        public async Task GetAccountByIdAsync_ExistingId_ReturnsAccount()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authRepository.GetAccountByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.AccountId);
        }

        [Fact]
        public async Task GetAccountByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _authRepository.GetAccountByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region SaveChangesAsync
        [Fact]
        public async Task SaveChangesAsync_WithChanges_SavesSuccessfully()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            _context.Accounts.Add(account);

            // Act
            await _authRepository.SaveChangesAsync();

            // Assert
            var result = await _context.Accounts.FindAsync(1);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SaveChangesAsync_NoChanges_Succeeds()
        {
            // Arrange

            // Act & Assert
            await _authRepository.SaveChangesAsync(); 
        }
        #endregion

        #region GetActiveUserOtpAsync
        [Fact]
        public async Task GetActiveUserOtpAsync_ExistingActiveOtp_ReturnsOtp()
        {
            // Arrange
            var otp = new UserOtp { AccountId = 1, OtpCode = "123456", ExpiresAt = DateTime.UtcNow.AddMinutes(5) };
            _context.UserOtps.Add(otp);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authRepository.GetActiveUserOtpAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("123456", result.OtpCode);
        }

        [Fact]
        public async Task GetActiveUserOtpAsync_ExpiredOtp_ReturnsNull()
        {
            // Arrange
            var otp = new UserOtp { AccountId = 1, OtpCode = "123456", ExpiresAt = DateTime.UtcNow.AddMinutes(-5) };
            _context.UserOtps.Add(otp);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authRepository.GetActiveUserOtpAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetActiveUserOtpAsync_NonExistingAccountId_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _authRepository.GetActiveUserOtpAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion
    }
}