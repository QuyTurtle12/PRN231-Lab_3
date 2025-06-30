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
                decimal? totalAmount = 0;

                // Calculate total amount and validate order items
                foreach (CartItem item in orderDto.OrderItems)
                {
                    Orchid? orchid = _unitOfWork.GetDAO<Orchid>().Entities.FirstOrDefault(o => o.OrchidId == item.OrchidId);
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

                // Create new order
                var order = new Order
                {
                    AccountId = orderDto.AccountId,
                    OrderDate = DateOnly.FromDateTime(DateTime.Now),
                    OrderStatus = OrderStatusEnum.Pending.ToString(),
                    TotalAmount = totalAmount
                };

                // Create order details
                var orderDetails = orderDto.OrderItems.Select(item => new OrderDetail
                {
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

                    _unitOfWork.CommitTransaction();
                    return order.Id;
                }
                catch
                {
                    _unitOfWork.RollBack();
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PaginatedList<Order>> GetAllAsync(int pageIndex, int pageNumber, string? idSearch, DateOnly? fromDate, DateOnly? toDate, OrderStatusEnum? status)
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
            query = query.OrderByDescending(o => o.OrderDate);

            // Convert query to paginated list
            PaginatedList<Order> result = await _unitOfWork.GetDAO<Order>().GetPagging(query, pageIndex, pageNumber);

            return result;
        }

        public async Task<bool> UpdateAsync(int orderId, UpdateOrderDTO orderDto)
        {
            try
            {
                // Get existing order
                var order = await _unitOfWork.GetDAO<Order>()
                    .Entities
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return false;

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

                    return true;
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
