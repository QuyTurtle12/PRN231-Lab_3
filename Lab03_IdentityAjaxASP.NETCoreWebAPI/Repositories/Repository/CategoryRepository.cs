using BusinessObjects;
using DataAccess.DTO.Category;
using DataAccess.Interface;
using DataAccess.Paginated;
using Repositories.Interface;

namespace Repositories.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IUOW _unitOfWork;

        public CategoryRepository(IUOW unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task DeleteAsync(int id)
        {
            // Get the existing category by ID
            Category? category = _unitOfWork.GetDAO<Category>().Entities.FirstOrDefault(c => c.CategoryId == id);

            // Check if the category exists
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            // Remove the category from the database
            await _unitOfWork.GetDAO<Category>().DeleteAsync(category);
            await _unitOfWork.SaveAsync();
        }

        public async Task<PaginatedList<Category>> GetAllAsync(int pageIndex, int pageNumber, string? idSearch, string? nameSearch)
        {
            // Validate inputs
            if (pageIndex < 1) pageIndex = 1;
            if (pageNumber < 1) pageNumber = 10;

            // Initialize the queryable collection from the DAO
            IQueryable<Category> query = _unitOfWork.GetDAO<Category>().Entities;

            // Apply Filters
            if (!string.IsNullOrWhiteSpace(idSearch))
            {
                query = query.Where(c => c.CategoryId.ToString() == idSearch);
            }

            if (!string.IsNullOrEmpty(nameSearch))
            {
                query = query.Where(c => c.CategoryName.Contains(nameSearch));
            }

            query = query.OrderByDescending(c => c.CategoryName);

            // Convert query to paginated list
            PaginatedList<Category> result = await _unitOfWork.GetDAO<Category>().GetPagging(query, pageIndex, pageNumber);

            return result;
        }

        public async Task InsertAsync(CreateCategoryDTO categoryDTO)
        {
            // Add DTO to Entity
            Category category = new Category
            {
                CategoryName = categoryDTO.Name
            };

            // Generate a new CategoryId
            category.CategoryId = _unitOfWork.GetDAO<Category>()
                .Entities
                .Select(c => c.CategoryId)
                .Max() + 1;

            // Insert the new category into the database
            await _unitOfWork.GetDAO<Category>().InsertAsync(category);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(int id, UpdateCategoryDTO categoryDTO)
        {
            Category? category = _unitOfWork.GetDAO<Category>().Entities.FirstOrDefault(c => c.CategoryId == id);

            // Check if the category exists
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            // Update the category properties
            category.CategoryName = categoryDTO.Name;

            // Save changes to the database
            await _unitOfWork.GetDAO<Category>().UpdateAsync(category);
            await _unitOfWork.SaveAsync();
        }
    }
}
