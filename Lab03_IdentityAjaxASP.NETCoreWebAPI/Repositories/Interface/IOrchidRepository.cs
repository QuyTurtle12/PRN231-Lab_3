using BusinessObjects;
using DataAccess.Paginated;

namespace Repositories.Interface
{
    public interface IOrchidRepository
    {
        Task<PaginatedList<Orchid>> GetAllAsync(int pageIndex, int pageNumber, string? idSearch, string? nameSearch, string? categorySearch, bool? isNatural);
        Task InsertAsync(Orchid orchid);
        Task UpdateAsync(Orchid orchid);
        Task DeleteAsync(int id);
    }
}
