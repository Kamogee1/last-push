using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Singular_Systems_SelfKiosk_Software.Models;
using SingularSystems_SelfKiosk_Software.Data;
using SingularSystems_SelfKiosk_Software.DTO;
using SingularSystems_SelfKiosk_Software.DTOs;
using SingularSystems_SelfKiosk_Software.Models;

namespace SingularSystems_SelfKiosk_Software.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    [ApiExplorerSettings(GroupName = "v1")] // optional for versioning
    public class OrderController : Controller 
    {
      private readonly DataContext _context;
        public OrderController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Order
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                   .ThenInclude(oi => oi.Product)
                .Select(o => new OrderDTO
                {
                    OrderId = o.OrderId,
                    UserId = o.UserId,
                    OrderDate = o.OrderDate,
                    CustomerTransactionId = o.CustomerTransactionId,
                    OrderTotalAmount = o.OrderTotalAmount,
                    OrderStatus = o.OrderStatus,
                    OrderItems = o.OrderItems.Select(item => new OrderItemDTO
                    {
                        OrderItemId = item.OrderItemId,
                        ProductId = item.ProductId,
                        OrderId = item.OrderId,
                        OrderItemsQuantity = item.OrderItemsQuantity,
                        OrderItemSubtotal = item.OrderItemSubtotal,
                        ProductName = item.Product.ProductName
                    }).ToList()

                }).ToListAsync();

            return Ok(orders);


        }
        // GET: api/Order/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDTO>> GetOrder([FromRoute] int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                  .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }
            var dto = new OrderDTO
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                CustomerTransactionId = order.CustomerTransactionId,
                OrderTotalAmount = order.OrderTotalAmount,
                OrderStatus = order.OrderStatus,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId,
                    OrderItemsQuantity = oi.OrderItemsQuantity,
                    OrderItemSubtotal = oi.OrderItemSubtotal

                }).ToList()
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDTO>> CreateOrder(OrderCreateDTO orderDto)
        {
            if (orderDto == null || orderDto.OrderItems == null || !orderDto.OrderItems.Any())
            {
                return BadRequest("Order must contain at least one order item.");
            }

            // Step 1: Create the Order entity
            var order = new Order
            {
                UserId = orderDto.UserId,
                OrderDate = DateTime.Now,
                OrderTotalAmount = orderDto.OrderTotalAmount,
                OrderStatus = orderDto.OrderStatus,
                OrderItems = orderDto.OrderItems.Select(oi => new OrderItem
                {
                    ProductId = oi.ProductId,
                    OrderItemsQuantity = oi.OrderItemsQuantity,
                    OrderItemSubtotal = oi.OrderItemSubtotal
                }).ToList()
            };

            // Step 2: Check and deduct from user's wallet
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == orderDto.UserId);
            if (wallet == null)
            {
                return BadRequest("Wallet not found for user.");
            }

            if (wallet.Balance < orderDto.OrderTotalAmount)
            {
                return BadRequest("Insufficient wallet balance.");
            }

            wallet.Balance -= orderDto.OrderTotalAmount;
            _context.Wallets.Update(wallet);

            // Step 3: Save the order to get OrderId
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Step 4: Create the CustomerTransaction for this order
            var transaction = new CustomerTransaction
            {
                WalletId = wallet.Id,
                OrderId = order.OrderId,
                Amount = order.OrderTotalAmount,
                Type = "Order",
                Date = DateTime.UtcNow,
                Status = "Completed"
            };

            _context.CustomerTransactions.Add(transaction);

            // Step 5: Link transaction to order (optional, if you keep a FK)
            order.CustomerTransactionId = transaction.CustomerTransactionId;
            _context.Orders.Update(order);

            await _context.SaveChangesAsync();

            // Step 6: Build response DTO
            var createdOrderDto = new OrderDTO
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                CustomerTransactionId = transaction.CustomerTransactionId,
                OrderTotalAmount = order.OrderTotalAmount,
                OrderStatus = order.OrderStatus,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId,
                    OrderId = oi.OrderId,
                    OrderItemsQuantity = oi.OrderItemsQuantity,
                    OrderItemSubtotal = oi.OrderItemSubtotal,
                    ProductName = _context.Products
                    .Where(p => p.ProductId == oi.ProductId)
                    .Select(p => p.ProductName)
                    .FirstOrDefault()
                }).ToList()
            };

            return CreatedAtAction(nameof(CreateOrder), new { id = order.OrderId }, createdOrderDto);
        }
        // PUT: api/Orders/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, OrderUpdateDTO orderDto)
        {
            if (id != orderDto.OrderId)
                return BadRequest("Order ID mismatch.");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            // Update fields
            order.UserId = orderDto.UserId;
            order.CustomerTransactionId = orderDto.CustomerTransactionId;
            order.OrderTotalAmount = orderDto.OrderTotalAmount;
            order.OrderStatus = orderDto.OrderStatus;

            _context.OrderItems.RemoveRange(order.OrderItems);  // Remove old items

            order.OrderItems = orderDto.OrderItems?.Select(oi => new OrderItem
            {
                ProductId = oi.ProductId,
                OrderItemsQuantity = oi.OrderItemsQuantity,
                // OrderItemSubtotal can be calculated server-side
                OrderId = id
            }).ToList() ?? new List<OrderItem>();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
