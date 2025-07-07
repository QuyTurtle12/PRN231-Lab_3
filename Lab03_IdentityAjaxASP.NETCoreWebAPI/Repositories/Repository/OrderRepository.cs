using BusinessObjects;
using DataAccess.Constant.Enum;
using DataAccess.DTO.Order;
using DataAccess.Interface;
using DataAccess.Paginated;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;

namespace Repositories.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IUOW _unitOfWork;

        public OrderRepository(IUOW unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> InsertAsync(CreateOrderDTO orderDto)
        {
            try
            {
                // Create new order
                var order = new Order
                {
                    AccountId = orderDto.AccountId,
                    OrderDate = DateOnly.FromDateTime(DateTime.Now),
                    OrderStatus = OrderStatusEnum.Pending.ToString(),
                    TotalAmount = orderDto.TotalAmount
                };

                order.Id = _unitOfWork.GetDAO<Order>()
                    .Entities
                    .Select(o => o.Id)
                    .Max() + 1;

                int lastOrderDetailId = _unitOfWork.GetDAO<OrderDetail>()
                    .Entities
                    .Max(od => od.Id);

                // Create order details
                var orderDetails = orderDto.OrderItems.Select((item, index) => new OrderDetail
                {
                    Id = lastOrderDetailId + index + 1,
                    OrchidId = item.OrchidId,
                    Quantity = item.Quantity,
                    Price = _unitOfWork.GetDAO<Orchid>().Entities
                        .Where(o => o.OrchidId == item.OrchidId)
                        .Select(o => o.Price)
                        .FirstOrDefault() * item.Quantity
                }).ToList();

                try
                {
                    // Insert order
                    await _unitOfWork.GetDAO<Order>().InsertAsync(order);
                    await _unitOfWork.SaveAsync();

                    // Set OrderId for details and insert them
                    orderDetails.ForEach(detail => detail.OrderId = order.Id);
                    await _unitOfWork.GetDAO<OrderDetail>().InsertRangeAsync(orderDetails);
                    await _unitOfWork.SaveAsync();

                    return order.Id;
                }
                catch
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PaginatedList<Order>> GetAllAsync(int pageIndex, int pageNumber, string? idSearch, string? customerSearch, DateOnly? fromDate, DateOnly? toDate, OrderStatusEnum? status)
        {
            // Validate inputs
            if (pageIndex < 1) pageIndex = 1;
            if (pageNumber < 1) pageNumber = 10;

            // Initialize the queryable collection from the DAO
            IQueryable<Order> query = _unitOfWork.GetDAO<Order>()
                .Entities
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Orchid)
                .Select(o => new Order
                {
                    Id = o.Id,
                    //OrderDetails = o.OrderDetails,
                    OrderDetails = (ICollection<OrderDetail>)o.OrderDetails.Select(od => new OrderDetail
                    {
                        Id = od.Id,
                        OrchidId = od.OrchidId,
                        Orchid = od.Orchid,
                        Quantity = od.Quantity,
                        Price = od.Price,
                    }),
                    AccountId = o.AccountId,
                    Account = o.Account,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus
                });

            // Apply Filters
            if (!string.IsNullOrWhiteSpace(idSearch))
            {
                query = query.Where(o => o.Id.ToString() == idSearch);
            }

            if (!string.IsNullOrWhiteSpace(customerSearch))
            {
                query = query.Where(o => o.Account!.Email == customerSearch);
            } 

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= toDate.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.OrderStatus == status.ToString());
            }

            // Order by OrderDate descending
            query = query.OrderByDescending(o => o.Id);

            // Convert query to paginated list
            PaginatedList<Order> result = await _unitOfWork.GetDAO<Order>().GetPagging(query, pageIndex, pageNumber);

            return result;
        }

        public async Task UpdateAsync(int orderId, UpdateOrderDTO orderDto)
        {
            try
            {
                // Get existing order
                var order = await _unitOfWork.GetDAO<Order>()
                    .Entities
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    throw new Exception($"Order with ID {orderId} not found.");

                try
                {
                    // Update order status if provided
                    if (!string.IsNullOrEmpty(orderDto.OrderStatus))
                    {
                        if (Enum.TryParse<OrderStatusEnum>(orderDto.OrderStatus, out var status))
                        {
                            order.OrderStatus = status.ToString();
                        }
                        else
                        {
                            throw new Exception("Invalid order status");
                        }
                    }

                    // Update order items if provided
                    if (orderDto.OrderItems != null && orderDto.OrderItems.Any())
                    {
                        decimal? totalAmount = 0;

                        // Validate and calculate new total amount
                        foreach (CartItem item in orderDto.OrderItems)
                        {
                            Orchid? orchid = await _unitOfWork.GetDAO<Orchid>()
                                .Entities
                                .FirstOrDefaultAsync(o => o.OrchidId == item.OrchidId);

                            if (orchid == null)
                            {
                                throw new Exception($"Orchid with ID {item.OrchidId} not found.");
                            }

                            if (item.Quantity <= 0)
                            {
                                throw new Exception($"Quantity for Orchid ID {item.OrchidId} must be greater than zero.");
                            }

                            totalAmount += orchid.Price * item.Quantity;
                        }

                        // Remove existing order details
                        var existingDetails = order.OrderDetails.ToList();
                        foreach (var detail in existingDetails)
                        {
                            await _unitOfWork.GetDAO<OrderDetail>().DeleteAsync(detail);
                        }

                        // Create new order details
                        var newDetails = orderDto.OrderItems.Select(item => new OrderDetail
                        {
                            OrderId = orderId,
                            OrchidId = item.OrchidId,
                            Quantity = item.Quantity,
                            Price = _unitOfWork.GetDAO<Orchid>().Entities
                                .Where(o => o.OrchidId == item.OrchidId)
                                .Select(o => o.Price)
                                .FirstOrDefault() * item.Quantity
                        }).ToList();

                        // Add new details
                        await _unitOfWork.GetDAO<OrderDetail>().InsertRangeAsync(newDetails);

                        // Update total amount
                        order.TotalAmount = totalAmount;
                    }

                    // Update order
                    await _unitOfWork.GetDAO<Order>().UpdateAsync(order);
                    await _unitOfWork.SaveAsync();
                }
                catch
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
