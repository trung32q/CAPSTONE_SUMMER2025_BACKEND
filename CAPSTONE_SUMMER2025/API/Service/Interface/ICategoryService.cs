using API.DTO.CategoryDTO;

namespace API.Service.Interface
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllAsync();
        Task<CategoryDTO> GetByIdAsync(int id);
        Task<CategoryDTO> AddAsync(CreateCategoryDTO dto);
        Task<CategoryDTO> UpdateAsync(int id, CreateCategoryDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
