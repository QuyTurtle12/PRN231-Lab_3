using DataAccess.Constant.Enum;
using DataAccess.DTO.Order;
using DataAccess.Paginated;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interface;

namespace Lab03_IdentityAjaxASP.NETCoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders(int pageIndex = 1, int pageNumber = 10, string? idSearch = null, DateOnly? fromDate = null, DateOnly? toDate = null, OrderStatusEnum? status = null)
        {
            try
            {
                var orders = await _orderRepository.GetAllAsync(pageIndex, pageNumber, idSearch, fromDate, toDate, status);
                
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDTO orderDto)
        {
            try
            {
                if (orderDto == null)
                {
                    return BadRequest("Order data is required.");
                }
                var orderId = await _orderRepository.InsertAsync(orderDto);
                return CreatedAtAction(nameof(GetAllOrders), new { id = orderId }, orderId);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(int orderId, UpdateOrderDTO orderDto)
        {
            try
            {
                if (orderDto == null)
                {
                    return BadRequest("Order data is required.");
                }
                var result = await _orderRepository.UpdateAsync(orderId, orderDto);
                if (!result)
                {
                    return NotFound($"Order with ID {orderId} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
