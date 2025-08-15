using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO.Admin;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Utils.Constants;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using UnitTest;
using Xunit;

namespace UnitTest.Repositories
{
    public class AdminRepositoryTest
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly IAdminRepository _adminRepository;

        public AdminRepositoryTest()
        {
            _context = TestHelper.CreateInMemoryDbContext();
            _adminRepository = new AdminRepository(_context);
        }

        #region GetCountAccountActiveAsync
        [Fact]
        public async Task GetCountAccountActiveAsync_ReturnsCorrectCount()
        {
            // Arrange
            var accounts = new List<Account>
            {
                new Account { AccountId = 1, Status = AccountStatusConst.VERIFIED, CreatedAt = DateTime.UtcNow },
                new Account { AccountId = 2, Status = AccountStatusConst.UNVERIFIED, CreatedAt = DateTime.UtcNow },
                new Account { AccountId = 3, Status = AccountStatusConst.BANNED, CreatedAt = DateTime.UtcNow }
            };
            _context.Accounts.AddRange(accounts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _adminRepository.GetCountAccountActiveAsync();

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetCountAccountActiveAsync_EmptyAccounts_ReturnsZero()
        {
            // Arrange

            // Act
            var result = await _adminRepository.GetCountAccountActiveAsync();

            // Assert
            Assert.Equal(0, result);
        }
        #endregion

        #region GetCountAccountAsync
        [Fact]
        public async Task GetCountAccountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var accounts = new List<Account>
            {
                new Account { AccountId = 1, Status = AccountStatusConst.VERIFIED, CreatedAt = DateTime.UtcNow },
                new Account { AccountId = 2, Status = AccountStatusConst.BANNED, CreatedAt = DateTime.UtcNow }
            };
            _context.Accounts.AddRange(accounts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _adminRepository.GetCountAccountAsync();

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetCountAccountAsync_EmptyAccounts_ReturnsZero()
        {
            // Arrange

            // Act
            var result = await _adminRepository.GetCountAccountAsync();

            // Assert
            Assert.Equal(0, result);
        }
        #endregion

        #region GetStartupCountAsync
        [Fact]
        public async Task GetStartupCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var startups = new List<Startup>
            {
                new Startup { StartupId = 1, CreateAt = DateTime.UtcNow },
                new Startup { StartupId = 2, CreateAt = DateTime.UtcNow }
            };
            _context.Startups.AddRange(startups);
            await _context.SaveChangesAsync();

            // Act
            var result = await _adminRepository.GetStartupCountAsync();

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GetStartupCountAsync_EmptyStartups_ReturnsZero()
        {
            // Arrange

            // Act
            var result = await _adminRepository.GetStartupCountAsync();

            // Assert
            Assert.Equal(0, result);
        }
        #endregion

        #region GetAccountsCreatedLast7DaysAsync
        [Fact]
        public async Task GetAccountsCreatedLast7DaysAsync_ReturnsCorrectStats()
        {
            // Arrange
            var today = DateTime.Today;
            var accounts = new List<Account>
            {
                new Account { AccountId = 1, CreatedAt = today.AddDays(-5), Status = AccountStatusConst.VERIFIED },
                new Account { AccountId = 2, CreatedAt = today.AddDays(-5), Status = AccountStatusConst.UNVERIFIED },
                new Account { AccountId = 3, CreatedAt = today.AddDays(-3), Status = AccountStatusConst.VERIFIED },
                new Account { AccountId = 4, CreatedAt = today.AddDays(-10), Status = AccountStatusConst.VERIFIED }
            };
            _context.Accounts.AddRange(accounts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _adminRepository.GetAccountsCreatedLast7DaysAsync();

            // Assert
            Assert.Equal(3, result.TotalNewAccountCount);
            Assert.Equal(7, result.DailyAccountStats.Count);
            Assert.Equal(2, result.DailyAccountStats.First(x => x.Date == today.AddDays(-5)).AccountCount);
            Assert.Equal(1, result.DailyAccountStats.First(x => x.Date == today.AddDays(-3)).AccountCount);
            Assert.All(result.DailyAccountStats.Where(x => x.Date != today.AddDays(-5) && x.Date != today.AddDays(-3)), x => Assert.Equal(0, x.AccountCount));
        }

        [Fact]
        public async Task GetAccountsCreatedLast7DaysAsync_NoAccounts_ReturnsZeroStats()
        {
            // Arrange

            // Act
            var result = await _adminRepository.GetAccountsCreatedLast7DaysAsync();

            // Assert
            Assert.Equal(0, result.TotalNewAccountCount);
            Assert.Equal(7, result.DailyAccountStats.Count);
            Assert.All(result.DailyAccountStats, x => Assert.Equal(0, x.AccountCount));
        }
        #endregion

        #region GetStartupsCreatedLast7DaysAsync
        [Fact]
        public async Task GetStartupsCreatedLast7DaysAsync_ReturnsCorrectStats()
        {
            // Arrange
            var today = DateTime.Today;
            var startups = new List<Startup>
            {
                new Startup { StartupId = 1, CreateAt = today.AddDays(-4) },
                new Startup { StartupId = 2, CreateAt = today.AddDays(-4) },
                new Startup { StartupId = 3, CreateAt = today.AddDays(-2) },
                new Startup { StartupId = 4, CreateAt = today.AddDays(-10) }
            };
            _context.Startups.AddRange(startups);
            await _context.SaveChangesAsync();

            // Act
            var result = await _adminRepository.GetStartupsCreatedLast7DaysAsync();

            // Assert
            Assert.Equal(3, result.TotalNewStartupCount);
            Assert.Equal(7, result.DailyStartupStats.Count);
            Assert.Equal(2, result.DailyStartupStats.First(x => x.Date == today.AddDays(-4)).StartupCount);
            Assert.Equal(1, result.DailyStartupStats.First(x => x.Date == today.AddDays(-2)).StartupCount);
            Assert.All(result.DailyStartupStats.Where(x => x.Date != today.AddDays(-4) && x.Date != today.AddDays(-2)), x => Assert.Equal(0, x.StartupCount));
        }

        [Fact]
        public async Task GetStartupsCreatedLast7DaysAsync_NullCreateAt_ReturnsZeroStats()
        {
            // Arrange
            var startups = new List<Startup>
            {
                new Startup { StartupId = 1, CreateAt = null },
                new Startup { StartupId = 2, CreateAt = null }
            };
            _context.Startups.AddRange(startups);
            await _context.SaveChangesAsync();

            // Act
            var result = await _adminRepository.GetStartupsCreatedLast7DaysAsync();

            // Assert
            Assert.Equal(0, result.TotalNewStartupCount);
            Assert.Equal(7, result.DailyStartupStats.Count);
            Assert.All(result.DailyStartupStats, x => Assert.Equal(0, x.StartupCount));
        }

        [Fact]
        public async Task GetStartupsCreatedLast7DaysAsync_NoStartups_ReturnsZeroStats()
        {
            // Arrange

            // Act
            var result = await _adminRepository.GetStartupsCreatedLast7DaysAsync();

            // Assert
            Assert.Equal(0, result.TotalNewStartupCount);
            Assert.Equal(7, result.DailyStartupStats.Count);
            Assert.All(result.DailyStartupStats, x => Assert.Equal(0, x.StartupCount));
        }
        #endregion

        #region GetUserGrowthRateAsync
        [Fact]
        public async Task GetUserGrowthRateAsync_ReturnsCorrectGrowth()
        {
            // Arrange
            var today = DateTime.Today;
            var startOfThisWeek = today.AddDays(-(int)today.DayOfWeek + 1);
            var startOfLastWeek = startOfThisWeek.AddDays(-7);
            var accounts = new List<Account>
            {
                new Account { AccountId = 1, CreatedAt = startOfLastWeek.AddDays(1), Status = AccountStatusConst.VERIFIED },
                new Account { AccountId = 2, CreatedAt = startOfLastWeek.AddDays(2), Status = AccountStatusConst.VERIFIED },
                new Account { AccountId = 3, CreatedAt = startOfThisWeek, Status = AccountStatusConst.VERIFIED },
                new Account { AccountId = 4, CreatedAt = startOfThisWeek.AddDays(1), Status = AccountStatusConst.VERIFIED },
                new Account { AccountId = 5, CreatedAt = startOfThisWeek.AddDays(2), Status = AccountStatusConst.VERIFIED }
            };
            _context.Accounts.AddRange(accounts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _adminRepository.GetUserGrowthRateAsync();

            // Assert
            Assert.Equal(3, result.ThisWeek);
            Assert.Equal(2, result.LastWeek);
            Assert.Equal(50, result.GrowthPercent);
        }

        [Fact]
        public async Task GetUserGrowthRateAsync_NoAccounts_ReturnsZero()
        {
            // Arrange

            // Act
            var result = await _adminRepository.GetUserGrowthRateAsync();

            // Assert
            Assert.Equal(0, result.ThisWeek);
            Assert.Equal(0, result.LastWeek);
            Assert.Equal(0, result.GrowthPercent);
        }

        [Fact]
        public async Task GetUserGrowthRateAsync_OnlyLastWeekAccounts_ReturnsZeroGrowth()
        {
            // Arrange
            var today = DateTime.Today;
            var startOfThisWeek = today.AddDays(-(int)today.DayOfWeek + 1);
            var startOfLastWeek = startOfThisWeek.AddDays(-7);
            var accounts = new List<Account>
            {
                new Account { AccountId = 1, CreatedAt = startOfLastWeek.AddDays(1), Status = AccountStatusConst.VERIFIED },
                new Account { AccountId = 2, CreatedAt = startOfLastWeek.AddDays(2), Status = AccountStatusConst.VERIFIED }
            };
            _context.Accounts.AddRange(accounts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _adminRepository.GetUserGrowthRateAsync();

            // Assert
            Assert.Equal(0, result.ThisWeek);
            Assert.Equal(2, result.LastWeek);
            Assert.Equal(-100, result.GrowthPercent);
        }
        #endregion
    }
}