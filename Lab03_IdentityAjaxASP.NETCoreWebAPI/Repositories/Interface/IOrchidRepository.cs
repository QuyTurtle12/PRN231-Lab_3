using BusinessObjects;
using DataAccess.DTO.Orchid;
using DataAccess.Paginated;

namespace Repositories.Interface
{
    public interface IOrchidRepository
    {
        Task<PaginatedList<Orchid>> GetAllAsync(int pageIndex, int pageNumber, string? idSearch, string? nameSearch, string? categorySearch, bool? isNatural);
        Task InsertAsync(CreateOrchidDTO orchid);
        Task UpdateAsync(int id, UpdateOrchidDTO orchid);
        Task DeleteAsync(int id);
    }
}
