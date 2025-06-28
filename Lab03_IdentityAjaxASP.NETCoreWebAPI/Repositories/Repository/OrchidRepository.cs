using BusinessObjects;
using DataAccess.Interface;
using DataAccess.Paginated;
using Repositories.Interface;

namespace Repositories.Repository
{
    public class OrchidRepository : IOrchidRepository
    {
        private readonly IUOW _unitOfWork;

        public OrchidRepository(IUOW unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginatedList<Orchid>> GetAllAsync(int pageIndex, int pageNumber, string? idSearch, string? nameSearch, string? categorySearch, bool? isNatural)
        {
            // Validate inputs
            if (pageIndex < 1) pageIndex = 1;
            if (pageNumber < 1) pageNumber = 10;

            // Initialize the queryable collection from the DAO
            IQueryable<Orchid> query = _unitOfWork.GetDAO<Orchid>().Entities;

            // Apply Filters
            if (!string.IsNullOrWhiteSpace(idSearch))
            {
                query = query.Where(o => o.OrchidId.ToString() == idSearch);
            }

            if (!string.IsNullOrEmpty(nameSearch))
            {
                query = query.Where(o => o.OrchidName.Contains(nameSearch));
            }

            if (!string.IsNullOrEmpty(categorySearch))
            {
                query = query.Where(o => o.Category != null && o.Category.CategoryName.Contains(categorySearch));
            }

            if (isNatural.HasValue)
            {
                query = query.Where(o => o.IsNatural == isNatural.Value);
            }

            query.OrderByDescending(o => o.OrchidName);

            // Convert query to paginated list
            PaginatedList<Orchid> result = await _unitOfWork.GetDAO<Orchid>().GetPagging(query, pageIndex, pageNumber);

            return result;
        }

        public Task InsertAsync(Orchid orchid)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Orchid orchid)
        {
            throw new NotImplementedException();
        }
    }
}
