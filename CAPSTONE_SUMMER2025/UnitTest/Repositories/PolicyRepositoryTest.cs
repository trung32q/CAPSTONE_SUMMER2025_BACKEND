using API.Repositories;
using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using UnitTest;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTest.Repositories
{
    public class PolicyRepositoryTest
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly IPolicyRepository _policyRepository;

        public PolicyRepositoryTest()
        {
            _context = TestHelper.CreateInMemoryDbContext();
            _policyRepository = new PolicyRepository(_context);
        }

        #region GetAllPolicyTypeAsync
        [Fact]
        public async Task GetAllPolicyTypeAsync_ReturnsAllPolicyTypes()
        {
            // Arrange
            var policyTypes = new List<PolicyType>
            {
                new PolicyType { TypeName = "Type 1" },
                new PolicyType { TypeName = "Type 2" }
            };
            _context.PolicyTypes.AddRange(policyTypes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _policyRepository.GetAllPolicyTypeAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, pt => pt.TypeName == "Type 1");
            Assert.Contains(result, pt => pt.TypeName == "Type 2");
        }
        #endregion

        #region GetPolicyTypeByIdAsync
        [Fact]
        public async Task GetPolicyTypeByIdAsync_ValidId_ReturnsPolicyType()
        {
            // Arrange
            var policyType = new PolicyType { TypeName = "Type 1" };
            _context.PolicyTypes.Add(policyType);
            await _context.SaveChangesAsync();

            // Act
            var result = await _policyRepository.GetPolicyTypeByIdAsync(policyType.PolicyTypeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(policyType.PolicyTypeId, result.PolicyTypeId);
            Assert.Equal("Type 1", result.TypeName);
        }

        [Fact]
        public async Task GetPolicyTypeByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _policyRepository.GetPolicyTypeByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region AddPolicyTypeAsync
        [Fact]
        public async Task AddPolicyTypeAsync_ValidPolicyType_AddsSuccessfully()
        {
            // Arrange
            var policyType = new PolicyType { TypeName = "Type 1" };

            // Act
            await _policyRepository.AddPolicyTypeAsync(policyType);

            // Assert
            var savedPolicyType = await _context.PolicyTypes.FirstOrDefaultAsync(pt => pt.TypeName == "Type 1");
            Assert.NotNull(savedPolicyType);
            Assert.Equal("Type 1", savedPolicyType.TypeName);
        }
        #endregion

        #region UpdatePolicyTypeAsync
        [Fact]
        public async Task UpdatePolicyTypeAsync_ValidPolicyType_UpdatesSuccessfully()
        {
            // Arrange
            var policyType = new PolicyType { TypeName = "Type 1" };
            _context.PolicyTypes.Add(policyType);
            await _context.SaveChangesAsync();
            policyType.TypeName = "Updated Type";

            // Act
            await _policyRepository.UpdatePolicyTypeAsync(policyType);

            // Assert
            var updatedPolicyType = await _context.PolicyTypes.FindAsync(policyType.PolicyTypeId);
            Assert.NotNull(updatedPolicyType);
            Assert.Equal("Updated Type", updatedPolicyType.TypeName);
        }
        #endregion

        #region DeletePolicyTypeAsync
        [Fact]
        public async Task DeletePolicyTypeAsync_ValidId_DeletesSuccessfully()
        {
            // Arrange
            var policyType = new PolicyType
            {
                TypeName = "Type 1",
                Policies = new List<Policy>
                {
                    new Policy { Description = "Policy 1", PolicyTypeId = 1, IsActive = true, CreateAt = DateTime.UtcNow }
                }
            };
            _context.PolicyTypes.Add(policyType);
            await _context.SaveChangesAsync();

            // Act
            await _policyRepository.DeletePolicyTypeAsync(policyType.PolicyTypeId);

            // Assert
            Assert.Equal(0, await _context.PolicyTypes.CountAsync());
            Assert.Equal(0, await _context.Policies.CountAsync());
        }

        [Fact]
        public async Task DeletePolicyTypeAsync_NonExistingId_DoesNothing()
        {
            // Arrange

            // Act
            await _policyRepository.DeletePolicyTypeAsync(999);

            // Assert
            Assert.Equal(0, await _context.PolicyTypes.CountAsync());
        }
        #endregion

        #region GetAllPolicyAsync
        [Fact]
        public async Task GetAllPolicyAsync_ReturnsAllPolicies()
        {
            // Arrange
            var policyType = new PolicyType { TypeName = "Type 1" };
            _context.PolicyTypes.Add(policyType);
            await _context.SaveChangesAsync();
            var policies = new List<Policy>
            {
                new Policy { Description = "Policy 1", PolicyTypeId = policyType.PolicyTypeId, IsActive = true, CreateAt = DateTime.UtcNow },
                new Policy { Description = "Policy 2", PolicyTypeId = policyType.PolicyTypeId, IsActive = false, CreateAt = DateTime.UtcNow }
            };
            _context.Policies.AddRange(policies);
            await _context.SaveChangesAsync();

            // Act
            var result = await _policyRepository.GetAllPolicyAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Description == "Policy 1");
            Assert.Contains(result, p => p.Description == "Policy 2");
        }
        #endregion

        #region GetAllActivePolicyAsync
        [Fact]
        public async Task GetAllActivePolicyAsync_ReturnsActivePolicies()
        {
            // Arrange
            var policyType = new PolicyType { TypeName = "Type 1" };
            _context.PolicyTypes.Add(policyType);
            await _context.SaveChangesAsync();
            var policies = new List<Policy>
            {
                new Policy { Description = "Policy 1", PolicyTypeId = policyType.PolicyTypeId, IsActive = true, CreateAt = DateTime.UtcNow },
                new Policy { Description = "Policy 2", PolicyTypeId = policyType.PolicyTypeId, IsActive = false, CreateAt = DateTime.UtcNow }
            };
            _context.Policies.AddRange(policies);
            await _context.SaveChangesAsync();

            // Act
            var result = await _policyRepository.GetAllActivePolicyAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Policy 1", result[0].Description);
            Assert.True(result[0].IsActive);
        }
        #endregion

        #region GetPolicyByIdAsync
        [Fact]
        public async Task GetPolicyByIdAsync_ValidId_ReturnsPolicy()
        {
            // Arrange
            var policyType = new PolicyType { TypeName = "Type 1" };
            _context.PolicyTypes.Add(policyType);
            await _context.SaveChangesAsync();
            var policy = new Policy { Description = "Policy 1", PolicyTypeId = policyType.PolicyTypeId, IsActive = true, CreateAt = DateTime.UtcNow };
            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();

            // Act
            var result = await _policyRepository.GetPolicyByIdAsync(policy.PolicyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(policy.PolicyId, result.PolicyId);
            Assert.Equal(policyType.PolicyTypeId, result.PolicyTypeId);
            Assert.Equal("Policy 1", result.Description);
            Assert.True(result.IsActive);
            Assert.Equal(policy.CreateAt, result.CreateAt);
        }

        [Fact]
        public async Task GetPolicyByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _policyRepository.GetPolicyByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region AddPolicyAsync
        [Fact]
        public async Task AddPolicyAsync_ValidPolicy_AddsSuccessfully()
        {
            // Arrange
            var policyType = new PolicyType { TypeName = "Type 1" };
            _context.PolicyTypes.Add(policyType);
            await _context.SaveChangesAsync();
            var policy = new Policy { Description = "Policy 1", PolicyTypeId = policyType.PolicyTypeId, IsActive = true, CreateAt = DateTime.UtcNow };

            // Act
            await _policyRepository.AddPolicyAsync(policy);

            // Assert
            var savedPolicy = await _context.Policies.FirstOrDefaultAsync(p => p.Description == "Policy 1");
            Assert.NotNull(savedPolicy);
            Assert.Equal(policyType.PolicyTypeId, savedPolicy.PolicyTypeId);
            Assert.Equal("Policy 1", savedPolicy.Description);
            Assert.True(savedPolicy.IsActive);
            Assert.Equal(policy.CreateAt, savedPolicy.CreateAt);
        }
        #endregion

        #region UpdatePolicyAsync
        [Fact]
        public async Task UpdatePolicyAsync_ValidPolicy_UpdatesSuccessfully()
        {
            // Arrange
            var policyType = new PolicyType { TypeName = "Type 1" };
            _context.PolicyTypes.Add(policyType);
            await _context.SaveChangesAsync();
            var policy = new Policy { Description = "Policy 1", PolicyTypeId = policyType.PolicyTypeId, IsActive = true, CreateAt = DateTime.UtcNow };
            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();
            policy.Description = "Updated Policy";
            policy.IsActive = false;
            policy.CreateAt = DateTime.UtcNow.AddDays(1);

            // Act
            await _policyRepository.UpdatePolicyAsync(policy);

            // Assert
            var updatedPolicy = await _context.Policies.FindAsync(policy.PolicyId);
            Assert.NotNull(updatedPolicy);
            Assert.Equal("Updated Policy", updatedPolicy.Description);
            Assert.False(updatedPolicy.IsActive);
            Assert.Equal(policy.CreateAt, updatedPolicy.CreateAt);
        }
        #endregion

        #region DeletePolicyAsync
        [Fact]
        public async Task DeletePolicyAsync_ValidId_DeletesSuccessfully()
        {
            // Arrange
            var policyType = new PolicyType { TypeName = "Type 1" };
            _context.PolicyTypes.Add(policyType);
            await _context.SaveChangesAsync();
            var policy = new Policy { Description = "Policy 1", PolicyTypeId = policyType.PolicyTypeId, IsActive = true, CreateAt = DateTime.UtcNow };
            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();

            // Act
            await _policyRepository.DeletePolicyAsync(policy.PolicyId);

            // Assert
            Assert.Equal(0, await _context.Policies.CountAsync());
        }

        [Fact]
        public async Task DeletePolicyAsync_NonExistingId_DoesNothing()
        {
            // Arrange

            // Act
            await _policyRepository.DeletePolicyAsync(999);

            // Assert
            Assert.Equal(0, await _context.Policies.CountAsync());
        }
        #endregion

        #region GetAllPoliciesByPolicyTypeAsync
        [Fact]
        public async Task GetAllPoliciesByPolicyTypeAsync_ValidPolicyTypeId_ReturnsPolicies()
        {
            // Arrange
            var policyType = new PolicyType { TypeName = "Type 1" };
            _context.PolicyTypes.Add(policyType);
            await _context.SaveChangesAsync();
            var policies = new List<Policy>
            {
                new Policy { Description = "Policy 1", PolicyTypeId = policyType.PolicyTypeId, IsActive = true, CreateAt = DateTime.UtcNow },
                new Policy { Description = "Policy 2", PolicyTypeId = policyType.PolicyTypeId, IsActive = false, CreateAt = DateTime.UtcNow }
            };
            _context.Policies.AddRange(policies);
            await _context.SaveChangesAsync();

            // Act
            var result = await _policyRepository.GetAllPoliciesByPolicyTypeAsync(policyType.PolicyTypeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Description == "Policy 1");
            Assert.Contains(result, p => p.Description == "Policy 2");
        }
        #endregion
    }
}