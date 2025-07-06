using DataAccess.Constant.Enum;
using DataAccess.DTO.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interface;

namespace Lab03_IdentityAjaxASP.NETCoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private const string STAFF_ROLE = "3";

        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders(int pageIndex = 1, int pageNumber = 10, string? idSearch = null, string? customerSearch = null, DateOnly? fromDate = null, DateOnly? toDate = null, OrderStatusEnum? status = null)
        {
            try
            {
                var orders = await _orderRepository.GetAllAsync(pageIndex, pageNumber, idSearch, customerSearch, fromDate, toDate, status);
                
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = STAFF_ROLE)]
        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDTO orderDto)
        {
            try
            {
                if (orderDto == null)
                {
                    return BadRequest("Order data is required.");
                }
                await _orderRepository.InsertAsync(orderDto);
                return CreatedAtAction(nameof(GetAllOrders), new { id = 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = STAFF_ROLE)]
        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(int orderId, UpdateOrderDTO orderDto)
        {
            try
            {
                if (orderDto == null)
                {
                    return BadRequest("Order data is required.");
                }
                await _orderRepository.UpdateAsync(orderId, orderDto);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
