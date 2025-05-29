using API.DTO.CategoryDTO;
using API.Repositories.Interfaces;
using API.Service.Interface;
using Infrastructure.Models;

namespace API.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;

        public CategoryService(ICategoryRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
        {
            var data = await _repo.GetAllAsync();
            return data.Select(x => new CategoryDTO { Category_ID = x.CategoryId, Category_Name = x.CategoryName });
        }

        public async Task<CategoryDTO> GetByIdAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return null;
            return new CategoryDTO { Category_ID = category.CategoryId, Category_Name = category.CategoryName };
        }

        public async Task<CategoryDTO> AddAsync(CreateCategoryDTO dto)
        {
            var category = new Category { CategoryName = dto.Category_Name };
            var created = await _repo.AddAsync(category);
            return new CategoryDTO {Category_ID = created.CategoryId, Category_Name = created.CategoryName };
        }

        public async Task<CategoryDTO> UpdateAsync(int id, CreateCategoryDTO dto)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return null;
            category.CategoryName = dto.Category_Name;
            var updated = await _repo.UpdateAsync(category);
            return new CategoryDTO { Category_ID = updated.CategoryId, Category_Name = updated.CategoryName };
        }

        public async Task<bool> DeleteAsync(int id) =>
            await _repo.DeleteAsync(id);
    }
}
