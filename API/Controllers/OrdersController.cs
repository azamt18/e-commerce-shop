using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using AutoMapper;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace API.Controllers
{
    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public OrdersController(IOrderService orderService, IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(OrderDto orderDto)
        {
            var email = HttpContext.User?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

            var address = _mapper.Map<AddressDto, Address>(orderDto.ShipToAddress);

            var order = await _orderService.CreateOrderAsync(email, orderDto.DeliveryMethodId, orderDto.BasketId,
                address);

            if (order == null)
            {
                return BadRequest(new ApiResponse(400, "Problem creating order"));
            }

            return Ok(order);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrdersForUser()
        {
            var email = HttpContext.User?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var orders = await _orderService.GetOrdersForUserAsync(email);
            var result = _mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderDto>>(orders);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderByIdForUser(int id)
        {
            var email = HttpContext.User?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var order = await _orderService.GetOrderByIdAsync(id, email);
            if (order == null)
            {
                return NotFound(new ApiResponse(404));
            }

            var result = _mapper.Map<Order, OrderToReturnDto>(order);
            return Ok(result);
        }
        [HttpGet("deliveryMethods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
            var deliveryMethods = await _orderService.GetDeliveryMethodsAsync();
            return Ok(deliveryMethods);
        }
    }
}