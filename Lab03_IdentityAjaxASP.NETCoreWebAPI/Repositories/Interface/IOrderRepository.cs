using BusinessObjects;
using DataAccess.Constant.Enum;
using DataAccess.DTO.Order;
using DataAccess.Paginated;

namespace Repositories.Interface
{
    public interface IOrderRepository
    {
        Task InsertAsync(CreateOrderDTO orderDto);
        Task UpdateAsync(int orderId, UpdateOrderDTO orderDto);
        Task<PaginatedList<Order>> GetAllAsync(int pageIndex, int pageNumber, string? idSearch, string? customerSearch, DateOnly? fromDate, DateOnly? toDate, OrderStatusEnum? status);
    }
}
