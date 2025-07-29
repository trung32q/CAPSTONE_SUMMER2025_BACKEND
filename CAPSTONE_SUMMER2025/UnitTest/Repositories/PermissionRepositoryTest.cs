using API.Repositories;
using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using UnitTest;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace UnitTest.Repositories
{
    public class PermissionRepositoryTest
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly IPermissionRepository _permissionRepository;

        public PermissionRepositoryTest()
        {
            _context = TestHelper.CreateInMemoryDbContext();
            _permissionRepository = new PermissionRepository(_context);
        }

        #region GetByIdAsync
        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsPermission()
        {
            // Arrange
            var role = new RoleInStartup { RoleId = 1, RoleName = "Admin" };
            var permission = new PermissionInStartup
            {
                PermissionId = 1,
                RoleId = 1,
                CanManagePost = true,
                CanManageCandidate = false,
                CanManageChatRoom = true,
                CanManageMember = false,
                CanManageMilestone = true,
                Role = role
            };
            _context.RoleInStartups.Add(role);
            _context.PermissionInStartups.Add(permission);
            await _context.SaveChangesAsync();

            // Act
            var result = await _permissionRepository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PermissionId);
            Assert.Equal(1, result.RoleId);
            Assert.True(result.CanManagePost);
            Assert.False(result.CanManageCandidate);
            Assert.True(result.CanManageChatRoom);
            Assert.False(result.CanManageMember);
            Assert.True(result.CanManageMilestone);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _permissionRepository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region UpdateAsync
        [Fact]
        public async Task UpdateAsync_ValidPermission_UpdatesSuccessfully()
        {
            // Arrange
            var role = new RoleInStartup { RoleId = 1, RoleName = "Admin" };
            var permission = new PermissionInStartup
            {
                PermissionId = 1,
                RoleId = 1,
                CanManagePost = true,
                CanManageCandidate = false,
                CanManageChatRoom = true,
                CanManageMember = false,
                CanManageMilestone = true,
                Role = role
            };
            _context.RoleInStartups.Add(role);
            _context.PermissionInStartups.Add(permission);
            await _context.SaveChangesAsync();
            permission.CanManagePost = false;
            permission.CanManageCandidate = true;

            // Act
            await _permissionRepository.UpdateAsync(permission);

            // Assert
            var updatedPermission = await _context.PermissionInStartups.FindAsync(1);
            Assert.NotNull(updatedPermission);
            Assert.False(updatedPermission.CanManagePost);
            Assert.True(updatedPermission.CanManageCandidate);
            Assert.True(updatedPermission.CanManageChatRoom);
            Assert.False(updatedPermission.CanManageMember);
            Assert.True(updatedPermission.CanManageMilestone);
        }
        #endregion

        #region GetRoleIdByAccountIdAsync
        [Fact]
        public async Task GetRoleIdByAccountIdAsync_ValidAccountId_ReturnsRoleId()
        {
            // Arrange
            var account = TestHelper.CreateTestAccount(1, "test@example.com");
            var role = new RoleInStartup { RoleId = 1, RoleName = "Admin" };
            var member = new StartupMember
            {
                AccountId = 1,
                RoleId = 1,
                StartupId = 1
            };
            _context.Accounts.Add(account);
            _context.RoleInStartups.Add(role);
            _context.StartupMembers.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _permissionRepository.GetRoleIdByAccountIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task GetRoleIdByAccountIdAsync_NonExistingAccountId_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _permissionRepository.GetRoleIdByAccountIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetPermissionValueAsync
        [Fact]
        public async Task GetPermissionValueAsync_ValidRoleIdAndCanManagePostSelector_ReturnsPermissionValue()
        {
            // Arrange
            var role = new RoleInStartup { RoleId = 1, RoleName = "Admin" };
            var permission = new PermissionInStartup
            {
                PermissionId = 1,
                RoleId = 1,
                CanManagePost = true,
                CanManageCandidate = false,
                CanManageChatRoom = true,
                CanManageMember = false,
                CanManageMilestone = true,
                Role = role
            };
            _context.RoleInStartups.Add(role);
            _context.PermissionInStartups.Add(permission);
            await _context.SaveChangesAsync();
            Expression<Func<PermissionInStartup, bool>> selector = p => p.CanManagePost;

            // Act
            var result = await _permissionRepository.GetPermissionValueAsync(1, selector);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetPermissionValueAsync_ValidRoleIdAndCanManageCandidateSelector_ReturnsPermissionValue()
        {
            // Arrange
            var role = new RoleInStartup { RoleId = 1, RoleName = "Admin" };
            var permission = new PermissionInStartup
            {
                PermissionId = 1,
                RoleId = 1,
                CanManagePost = true,
                CanManageCandidate = false,
                CanManageChatRoom = true,
                CanManageMember = false,
                CanManageMilestone = true,
                Role = role
            };
            _context.RoleInStartups.Add(role);
            _context.PermissionInStartups.Add(permission);
            await _context.SaveChangesAsync();
            Expression<Func<PermissionInStartup, bool>> selector = p => p.CanManageCandidate;

            // Act
            var result = await _permissionRepository.GetPermissionValueAsync(1, selector);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetPermissionValueAsync_ValidRoleIdAndCanManageChatRoomSelector_ReturnsPermissionValue()
        {
            // Arrange
            var role = new RoleInStartup { RoleId = 1, RoleName = "Admin" };
            var permission = new PermissionInStartup
            {
                PermissionId = 1,
                RoleId = 1,
                CanManagePost = true,
                CanManageCandidate = false,
                CanManageChatRoom = true,
                CanManageMember = false,
                CanManageMilestone = true,
                Role = role
            };
            _context.RoleInStartups.Add(role);
            _context.PermissionInStartups.Add(permission);
            await _context.SaveChangesAsync();
            Expression<Func<PermissionInStartup, bool>> selector = p => p.CanManageChatRoom;

            // Act
            var result = await _permissionRepository.GetPermissionValueAsync(1, selector);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetPermissionValueAsync_ValidRoleIdAndCanManageMemberSelector_ReturnsPermissionValue()
        {
            // Arrange
            var role = new RoleInStartup { RoleId = 1, RoleName = "Admin" };
            var permission = new PermissionInStartup
            {
                PermissionId = 1,
                RoleId = 1,
                CanManagePost = true,
                CanManageCandidate = false,
                CanManageChatRoom = true,
                CanManageMember = false,
                CanManageMilestone = true,
                Role = role
            };
            _context.RoleInStartups.Add(role);
            _context.PermissionInStartups.Add(permission);
            await _context.SaveChangesAsync();
            Expression<Func<PermissionInStartup, bool>> selector = p => p.CanManageMember;

            // Act
            var result = await _permissionRepository.GetPermissionValueAsync(1, selector);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetPermissionValueAsync_ValidRoleIdAndCanManageMilestoneSelector_ReturnsPermissionValue()
        {
            // Arrange
            var role = new RoleInStartup { RoleId = 1, RoleName = "Admin" };
            var permission = new PermissionInStartup
            {
                PermissionId = 1,
                RoleId = 1,
                CanManagePost = true,
                CanManageCandidate = false,
                CanManageChatRoom = true,
                CanManageMember = false,
                CanManageMilestone = true,
                Role = role
            };
            _context.RoleInStartups.Add(role);
            _context.PermissionInStartups.Add(permission);
            await _context.SaveChangesAsync();
            Expression<Func<PermissionInStartup, bool>> selector = p => p.CanManageMilestone;

            // Act
            var result = await _permissionRepository.GetPermissionValueAsync(1, selector);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetPermissionValueAsync_NonExistingRoleId_ReturnsFalse()
        {
            // Arrange
            Expression<Func<PermissionInStartup, bool>> selector = p => p.CanManagePost;

            // Act
            var result = await _permissionRepository.GetPermissionValueAsync(999, selector);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region GetByRoleIdAsync
        [Fact]
        public async Task GetByRoleIdAsync_ValidRoleId_ReturnsPermission()
        {
            // Arrange
            var role = new RoleInStartup { RoleId = 1, RoleName = "Admin" };
            var permission = new PermissionInStartup
            {
                PermissionId = 1,
                RoleId = 1,
                CanManagePost = true,
                CanManageCandidate = false,
                CanManageChatRoom = true,
                CanManageMember = false,
                CanManageMilestone = true,
                Role = role
            };
            _context.RoleInStartups.Add(role);
            _context.PermissionInStartups.Add(permission);
            await _context.SaveChangesAsync();

            // Act
            var result = await _permissionRepository.GetByRoleIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PermissionId);
            Assert.Equal(1, result.RoleId);
            Assert.True(result.CanManagePost);
            Assert.False(result.CanManageCandidate);
            Assert.True(result.CanManageChatRoom);
            Assert.False(result.CanManageMember);
            Assert.True(result.CanManageMilestone);
        }

        [Fact]
        public async Task GetByRoleIdAsync_NonExistingRoleId_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _permissionRepository.GetByRoleIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion
    }
}