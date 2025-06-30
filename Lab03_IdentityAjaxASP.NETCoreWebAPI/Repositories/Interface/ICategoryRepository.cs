using BusinessObjects;
using DataAccess.DTO.Category;
using DataAccess.Paginated;

namespace Repositories.Interface
{
    public interface ICategoryRepository
    {
        public Task<PaginatedList<Category>> GetAllAsync(int pageIndex, int pageNumber, string? idSearch, string? nameSearch);

        public Task InsertAsync(CreateCategoryDTO category);

        public Task UpdateAsync(int id, UpdateCategoryDTO category);

        public Task DeleteAsync(int id);
    }
}
