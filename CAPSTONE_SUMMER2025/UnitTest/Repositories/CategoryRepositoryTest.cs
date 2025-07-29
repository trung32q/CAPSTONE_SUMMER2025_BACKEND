using API.Repositories;
using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using UnitTest;
using Xunit;
using System;

namespace UnitTest.Repositories
{
    public class CategoryRepositoryTest
    {
        private readonly CAPSTONE_SUMMER2025Context _context;
        private readonly ICategoryRepository _repository;

        public CategoryRepositoryTest()
        {
            _context = TestHelper.CreateInMemoryDbContext();
            _repository = new CategoryRepository(_context);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllCategories()
        {
            // Arrange
            var category1 = new Category { CategoryId = 1, CategoryName = "Technology" };
            var category2 = new Category { CategoryId = 2, CategoryName = "Finance" };
            _context.Categories.AddRange(category1, category2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, c => c.CategoryId == 1 && c.CategoryName == "Technology");
            Assert.Contains(result, c => c.CategoryId == 2 && c.CategoryName == "Finance");
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsCategory()
        {
            // Arrange
            var category = new Category { CategoryId = 1, CategoryName = "Technology" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CategoryId);
            Assert.Equal("Technology", result.CategoryName);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange & Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_NewCategory_ReturnsAddedCategory()
        {
            // Arrange
            var category = new Category { CategoryId = 1, CategoryName = "Technology" };

            // Act
            var result = await _repository.AddAsync(category);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CategoryId);
            Assert.Equal("Technology", result.CategoryName);
            Assert.Equal(1, await _context.Categories.CountAsync(c => c.CategoryId == 1));
        }

        [Fact]
        public async Task UpdateAsync_ExistingCategory_ReturnsUpdatedCategory()
        {
            // Arrange
            var category = new Category { CategoryId = 1, CategoryName = "Technology" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            category.CategoryName = "UpdatedTechnology";

            // Act
            var result = await _repository.UpdateAsync(category);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CategoryId);
            Assert.Equal("UpdatedTechnology", result.CategoryName);
            var updated = await _context.Categories.FindAsync(1);
            Assert.Equal("UpdatedTechnology", updated.CategoryName);
        }

        [Fact]
        public async Task DeleteAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            var category = new Category { CategoryId = 1, CategoryName = "Technology" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(1);

            // Assert
            Assert.True(result);
            Assert.Null(await _context.Categories.FindAsync(1));
        }

        [Fact]
        public async Task DeleteAsync_NonExistingId_ReturnsFalse()
        {
            // Arrange & Act
            var result = await _repository.DeleteAsync(999);

            // Assert
            Assert.False(result);
        }
    }
}