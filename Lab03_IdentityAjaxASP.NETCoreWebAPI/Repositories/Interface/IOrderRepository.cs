using BusinessObjects;
using DataAccess.Constant.Enum;
using DataAccess.DTO.Order;
using DataAccess.Paginated;

namespace Repositories.Interface
{
    public interface IOrderRepository
    {
        Task<int> InsertAsync(CreateOrderDTO orderDto);
        Task<bool> UpdateAsync(int orderId, UpdateOrderDTO orderDto);
        Task<PaginatedList<Order>> GetAllAsync(int pageIndex, int pageNumber, string? idSearch, DateOnly? fromDate, DateOnly? toDate, OrderStatusEnum? status);
    }
}
