using API.DTO.BioDTO;
using Infrastructure.Models;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace UnitTest.Repositories
{
    public class AccountRepositoryTest
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly IMapper _mapper;
        private readonly AccountRepository _accountRepository;

        public AccountRepositoryTest()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingAccount());
            });
            _mapper = mappingConfig.CreateMapper();
            _context = TestHelper.CreateInMemoryDbContext();
            _accountRepository = new AccountRepository(_mapper, _context);
        }

        #region GetAccountByEmailAsync
        [Fact]
        public async Task GetAccountByEmailAsync_ExistingEmail_ReturnsAccount()
        {
            // Arrange
            var email = "test@example.com";
            var account = TestHelper.CreateTestAccount(1, email);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.GetAccountByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task GetAccountByEmailAsync_NonExistingEmail_ReturnsNull()
        {
            // Arrange
            var email = "nonexistent@example.com";

            // Act
            var result = await _accountRepository.GetAccountByEmailAsync(email);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetAllAccountAsync
        [Fact]
        public async Task GetAllAccountAsync_WithAccounts_ReturnsList()
        {
            // Arrange
            var account1 = TestHelper.CreateTestAccount(1, "test1@example.com");
            var account2 = TestHelper.CreateTestAccount(2, "test2@example.com");
            _context.Accounts.AddRange(account1, account2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.GetAllAccountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
        #endregion

        #region GetAccountByAccountIDAsync
        [Fact]
        public async Task GetAccountByAccountIDAsync_ExistingId_ReturnsAccount()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.GetAccountByAccountIDAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.AccountId);
        }

        [Fact]
        public async Task GetAccountByAccountIDAsync_NonExistingId_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _accountRepository.GetAccountByAccountIDAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetFollowingAsync
        [Fact]
        public async Task GetFollowingAsync_ValidAccountId_ReturnsFollowingList()
        {
            // Arrange
            var follower = TestHelper.CreateTestAccount(1, "follower@example.com");
            var following = TestHelper.CreateTestAccount(2, "following@example.com");
            _context.Accounts.AddRange(follower, following);
            _context.Follows.Add(new Follow { FollowerAccountId = 1, FollowingAccountId = 2, FollowDate = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.GetFollowingAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(2, result[0].AccountId);
        }
        #endregion

        #region GetFollowersAsync
        [Fact]
        public async Task GetFollowersAsync_ValidAccountId_ReturnsFollowersList()
        {
            // Arrange
            var follower = TestHelper.CreateTestAccount(1, "follower@example.com");
            var following = TestHelper.CreateTestAccount(2, "following@example.com");
            _context.Accounts.AddRange(follower, following);
            _context.Follows.Add(new Follow { FollowerAccountId = 1, FollowingAccountId = 2, FollowDate = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.GetFollowersAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].AccountId);
        }
        #endregion

        #region GetAccountWithProfileByIdAsync
        [Fact]
        public async Task GetAccountWithProfileByIdAsync_ExistingId_ReturnsAccount()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.GetAccountWithProfileByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AccountProfile);
        }
        #endregion

        #region GetAccountWithBioByIdAsync
        [Fact]
        public async Task GetAccountWithBioByIdAsync_ExistingId_ReturnsAccount()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            account.Bio = new Bio { AccountId = 1 };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.GetAccountWithBioByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Bio);
        }
        #endregion

        #region SaveChangesAsync
        [Fact]
        public async Task SaveChangesAsync_WithChanges_SavesSuccessfully()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            account.Email = "updated@example.com";
            await _accountRepository.SaveChangesAsync();

            // Assert
            var updatedAccount = await _context.Accounts.FindAsync(1);
            Assert.Equal("updated@example.com", updatedAccount.Email);
        }
        #endregion

        #region UpdateBioAsync
        [Fact]
        public async Task UpdateBioAsync_ValidInput_ReturnsResAccountInfoDTO()
        {
            // Arrange
            var accountId = 1;
            var account = TestHelper.CreateTestAccount(accountId, "test@example.com");
            account.Bio = new Bio { AccountId = accountId };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var updateBioDto = new ReqUpdateBioDTO
            {
                IntroTitle = "New Intro",
                Position = "Developer",
                Workplace = "Tech Corp"
            };

            // Act
            var result = await _accountRepository.UpdateBioAsync(accountId, updateBioDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Intro", result.IntroTitle);
            Assert.Equal("Developer", result.Position);
            Assert.Equal("Tech Corp", result.Workplace);
        }
        #endregion

        #region ChangePasswordAsync
        [Fact]
        public async Task ChangePasswordAsync_ValidInput_ReturnsTrue()
        {
            // Arrange
            var accountId = 1;
            var oldPassword = "OldPassword123";
            var newPassword = "NewPassword123";
            var account = TestHelper.CreateTestAccount(accountId, "test@example.com");
            account.Password = BCrypt.Net.BCrypt.HashPassword(oldPassword);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var changePasswordDto = new ChangePasswordDTO
            {
                OldPassword = oldPassword,
                NewPassword = newPassword,
                ConfirmPassword = newPassword
            };

            // Act
            var result = await _accountRepository.ChangePasswordAsync(accountId, changePasswordDto);

            // Assert
            Assert.True(result);
            var updatedAccount = await _context.Accounts.FindAsync(accountId);
            Assert.True(BCrypt.Net.BCrypt.Verify(newPassword, updatedAccount.Password));
        }

        [Fact]
        public async Task ChangePasswordAsync_InvalidOldPassword_ReturnsFalse()
        {
            // Arrange
            var accountId = 1;
            var oldPassword = "OldPassword123";
            var newPassword = "NewPassword123";
            var account = TestHelper.CreateTestAccount(accountId, "test@example.com");
            account.Password = BCrypt.Net.BCrypt.HashPassword(oldPassword);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var changePasswordDto = new ChangePasswordDTO
            {
                OldPassword = "WrongPassword123", 
                NewPassword = newPassword,
                ConfirmPassword = newPassword
            };

            // Act
            var result = await _accountRepository.ChangePasswordAsync(accountId, changePasswordDto);

            // Assert
            Assert.False(result);
            var updatedAccount = await _context.Accounts.FindAsync(accountId);
            Assert.True(BCrypt.Net.BCrypt.Verify(oldPassword, updatedAccount.Password)); 
        }

        [Fact]
        public async Task ChangePasswordAsync_PasswordsNotMatch_ReturnsFalse()
        {
            // Arrange
            var accountId = 1;
            var oldPassword = "OldPassword123";
            var account = TestHelper.CreateTestAccount(accountId, "test@example.com");
            account.Password = BCrypt.Net.BCrypt.HashPassword(oldPassword);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var changePasswordDto = new ChangePasswordDTO
            {
                OldPassword = oldPassword,
                NewPassword = "NewPassword123",
                ConfirmPassword = "DifferentPassword123"
            };

            // Act
            var result = await _accountRepository.ChangePasswordAsync(accountId, changePasswordDto);

            // Assert
            Assert.False(result);
            var updatedAccount = await _context.Accounts.FindAsync(accountId);
            Assert.True(BCrypt.Net.BCrypt.Verify(oldPassword, updatedAccount.Password)); 
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
            var result = await _accountRepository.GetAccountByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.AccountId);
        }

        [Fact]
        public async Task GetAccountByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _accountRepository.GetAccountByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region IsBlockedAsync
        [Fact]
        public async Task IsBlockedAsync_ExistingBlock_ReturnsTrue()
        {
            // Arrange
            var blocker = TestHelper.CreateTestAccount(1, "blocker@example.com");
            var blocked = TestHelper.CreateTestAccount(2, "blocked@example.com");
            _context.Accounts.AddRange(blocker, blocked);
            _context.AccountBlocks.Add(new AccountBlock { BlockerAccountId = 1, BlockedAccountId = 2, BlockedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.IsBlockedAsync(1, 2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsBlockedAsync_NonExistingBlock_ReturnsFalse()
        {
            // Arrange
            var blocker = TestHelper.CreateTestAccount(1, "blocker@example.com");
            var blocked = TestHelper.CreateTestAccount(2, "blocked@example.com");
            _context.Accounts.AddRange(blocker, blocked);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.IsBlockedAsync(1, 2);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region FollowAsync
        [Fact]
        public async Task FollowAsync_ValidAccounts_ReturnsTrue()
        {
            // Arrange
            var follower = TestHelper.CreateTestAccount(1, "follower@example.com");
            var following = TestHelper.CreateTestAccount(2, "following@example.com");
            _context.Accounts.AddRange(follower, following);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.FollowAsync(1, 2);

            // Assert
            Assert.True(result);
            var follow = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerAccountId == 1 && f.FollowingAccountId == 2);
            Assert.NotNull(follow);
        }

        [Fact]
        public async Task FollowAsync_NonExistingFollower_ReturnsFalse()
        {
            // Arrange
            var following = TestHelper.CreateTestAccount(2, "following@example.com");
            _context.Accounts.Add(following);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.FollowAsync(999, 2);

            // Assert
            Assert.False(result);
            Assert.Empty(_context.Follows);
        }
        #endregion

        #region UnfollowAsync
        [Fact]
        public async Task UnfollowAsync_ValidAccounts_ReturnsTrue()
        {
            // Arrange
            var follower = TestHelper.CreateTestAccount(1, "follower@example.com");
            var following = TestHelper.CreateTestAccount(2, "following@example.com");
            _context.Accounts.AddRange(follower, following);
            _context.Follows.Add(new Follow { FollowerAccountId = 1, FollowingAccountId = 2, FollowDate = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.UnfollowAsync(1, 2);

            // Assert
            Assert.True(result);
            Assert.Empty(await _context.Follows.ToListAsync());
        }

        [Fact]
        public async Task UnfollowAsync_NonExistingFollow_ReturnsFalse()
        {
            // Arrange
            var follower = TestHelper.CreateTestAccount(1, "follower@example.com");
            var following = TestHelper.CreateTestAccount(2, "following@example.com");
            _context.Accounts.AddRange(follower, following);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.UnfollowAsync(1, 2);

            // Assert
            Assert.False(result);
            Assert.Empty(_context.Follows);
        }
        #endregion

        #region IsFollowingAsync
        [Fact]
        public async Task IsFollowingAsync_ExistingFollow_ReturnsTrue()
        {
            // Arrange
            var follower = TestHelper.CreateTestAccount(1, "follower@example.com");
            var following = TestHelper.CreateTestAccount(2, "following@example.com");
            _context.Accounts.AddRange(follower, following);
            _context.Follows.Add(new Follow { FollowerAccountId = 1, FollowingAccountId = 2, FollowDate = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.IsFollowingAsync(1, 2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsFollowingAsync_NonExistingFollow_ReturnsFalse()
        {
            // Arrange
            var follower = TestHelper.CreateTestAccount(1, "follower@example.com");
            var following = TestHelper.CreateTestAccount(2, "following@example.com");
            _context.Accounts.AddRange(follower, following);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.IsFollowingAsync(1, 2);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region GetSearchAccounts
        [Fact]
        public void GetSearchAccounts_ValidKeyword_ReturnsQueryable()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            account.AccountProfile.FirstName = "Nguyen";
            account.AccountProfile.LastName = "Van A";
            account.Bio = new Bio { Position = "Developer", Workplace = "Tech Corp", AccountId = 1 };
            _context.Accounts.Add(account);
            _context.SaveChanges(); 

            // Act
            var result = _accountRepository.GetSearchAccounts("nguyen", 0);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, a => a.FullName.Contains("Nguyen Van A"));
        }

        [Fact]
        public void GetSearchAccounts_EmptyKeyword_ReturnsAllAccounts()
        {
            // Arrange
            var account1 = TestHelper.CreateTestAccount(1, "test1@example.com");
            var account2 = TestHelper.CreateTestAccount(2, "test2@example.com");
            _context.Accounts.AddRange(account1, account2);
            _context.SaveChanges();

            // Act
            var result = _accountRepository.GetSearchAccounts("", 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }
        #endregion

        #region RecommendAccountsAsync
        [Fact]
        public async Task RecommendAccountsAsync_ValidInput_ReturnsPagedResult()
        {
            // Arrange
            var currentAccount = TestHelper.CreateTestAccount(1, "current@example.com");
            var recommendAccount = TestHelper.CreateTestAccount(2, "recommend@example.com");
            _context.Accounts.AddRange(currentAccount, recommendAccount);
            await _context.SaveChangesAsync();

            // Mock post interaction (giả lập tương tác để gợi ý)
            var post = new Post { PostId = 1, AccountId = 2 };
            _context.Posts.Add(post);
            _context.PostLikes.Add(new PostLike { PostId = 1, AccountId = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.RecommendAccountsAsync(1, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalCount >= 1);
            Assert.Contains(result.Items, a => a.AccountId == 2);
        }
        #endregion

        #region GetBlockAsync
        [Fact]
        public async Task GetBlockAsync_ExistingBlock_ReturnsBlock()
        {
            // Arrange
            var blocker = TestHelper.CreateTestAccount(1, "blocker@example.com");
            var blocked = TestHelper.CreateTestAccount(2, "blocked@example.com");
            _context.Accounts.AddRange(blocker, blocked);
            _context.AccountBlocks.Add(new AccountBlock { BlockerAccountId = 1, BlockedAccountId = 2, BlockedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.GetBlockAsync(1, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.BlockerAccountId);
            Assert.Equal(2, result.BlockedAccountId);
        }

        [Fact]
        public async Task GetBlockAsync_NonExistingBlock_ReturnsNull()
        {
            // Arrange
            var blocker = TestHelper.CreateTestAccount(1, "blocker@example.com");
            var blocked = TestHelper.CreateTestAccount(2, "blocked@example.com");
            _context.Accounts.AddRange(blocker, blocked);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.GetBlockAsync(1, 2);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region BlockAccountAsync
        [Fact]
        public async Task BlockAccountAsync_ValidInput_ReturnsTrue()
        {
            // Arrange
            var blocker = TestHelper.CreateTestAccount(1, "blocker@example.com");
            var blocked = TestHelper.CreateTestAccount(2, "blocked@example.com");
            _context.Accounts.AddRange(blocker, blocked);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.BlockAccountAsync(1, 2);

            // Assert
            Assert.True(result);
            var block = await _context.AccountBlocks.FirstOrDefaultAsync(b => b.BlockerAccountId == 1 && b.BlockedAccountId == 2);
            Assert.NotNull(block);
        }

        [Fact]
        public async Task BlockAccountAsync_ExistingBlock_ReturnsFalse()
        {
            // Arrange
            var blocker = TestHelper.CreateTestAccount(1, "blocker@example.com");
            var blocked = TestHelper.CreateTestAccount(2, "blocked@example.com");
            _context.Accounts.AddRange(blocker, blocked);
            _context.AccountBlocks.Add(new AccountBlock { BlockerAccountId = 1, BlockedAccountId = 2, BlockedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.BlockAccountAsync(1, 2);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region UnblockAccountAsync
        [Fact]
        public async Task UnblockAccountAsync_ValidInput_ReturnsTrue()
        {
            // Arrange
            var blocker = TestHelper.CreateTestAccount(1, "blocker@example.com");
            var blocked = TestHelper.CreateTestAccount(2, "blocked@example.com");
            _context.Accounts.AddRange(blocker, blocked);
            _context.AccountBlocks.Add(new AccountBlock { BlockerAccountId = 1, BlockedAccountId = 2, BlockedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.UnblockAccountAsync(1, 2);

            // Assert
            Assert.True(result);
            Assert.Empty(await _context.AccountBlocks.ToListAsync());
        }

        [Fact]
        public async Task UnblockAccountAsync_NonExistingBlock_ReturnsFalse()
        {
            // Arrange
            var blocker = TestHelper.CreateTestAccount(1, "blocker@example.com");
            var blocked = TestHelper.CreateTestAccount(2, "blocked@example.com");
            _context.Accounts.AddRange(blocker, blocked);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.UnblockAccountAsync(1, 2);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region GetBlockedAccountsAsync
        [Fact]
        public async Task GetBlockedAccountsAsync_ValidBlockerId_ReturnsList()
        {
            // Arrange
            var blocker = TestHelper.CreateTestAccount(1, "blocker@example.com");
            var blocked = TestHelper.CreateTestAccount(2, "blocked@example.com");
            _context.Accounts.AddRange(blocker, blocked);
            _context.AccountBlocks.Add(new AccountBlock { BlockerAccountId = 1, BlockedAccountId = 2, BlockedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountRepository.GetBlockedAccountsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(2, result[0].BlockedAccountId);
        }
        #endregion
    }
}