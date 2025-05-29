using API.Repositories.Interfaces;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class CategoryRepository:ICategoryRepository
    {
        private readonly CAPSTONE_SUMMER2025Context _context;

        public CategoryRepository(CAPSTONE_SUMMER2025Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync() =>
            await _context.Categories.ToListAsync();

        public async Task<Category> GetByIdAsync(int id) =>
            await _context.Categories.FindAsync(id);

        public async Task<Category> AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            _context.Categories.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
