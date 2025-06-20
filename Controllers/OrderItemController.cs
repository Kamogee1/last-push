using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SingularSystems_SelfKiosk_Software.DTO;
using Singular_Systems_SelfKiosk_Software.Models;
using SingularSystems_SelfKiosk_Software.Data;


namespace SingularSystems_SelfKiosk_Software.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    public class OrderItemController : ControllerBase

    {
        private readonly DataContext _context;
        public OrderItemController(DataContext context)
        {
            _context = context;
        }

        //GET: api/OrderItem 
        [HttpGet]

        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetOrderItems()
        {
            var items = await _context.OrderItems
                .Include(o => o.Product)
                .ToListAsync();

            return items.Select(item => new OrderItemDTO
            {
                OrderItemId = item.OrderItemId,
                ProductId = item.ProductId,
                OrderId = item.OrderId,
                OrderItemsQuantity = item.OrderItemsQuantity,
                OrderItemSubtotal = item.OrderItemSubtotal,
                ProductName = item.Product?.ProductName
            }).ToList();

        }



        //GET: api/OrderItem/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItemDTO>> GetOrderItem(int id)
        {
            var item = await _context.OrderItems
                    .Include(o => o.Product)
                    .FirstOrDefaultAsync(o => o.OrderItemId == id);


            if (item == null)
            {
                return NotFound();
            }
            return new OrderItemDTO
            {
                OrderItemId = item.OrderItemId,
                ProductId = item.ProductId,
                OrderId = item.OrderId,
                OrderItemsQuantity = item.OrderItemsQuantity,
                OrderItemSubtotal = item.OrderItemSubtotal,
                ProductName = item.Product?.ProductName
            };
        }

        //POST: api/OrderItem
        [HttpPost]

        public async Task<ActionResult<OrderItem>> PostOrderItem(OrderItemDTO dto)
        {
            var item = new OrderItem
            {
                ProductId = dto.ProductId,
                OrderId = dto.OrderId,
                OrderItemsQuantity = dto.OrderItemsQuantity,
                OrderItemSubtotal = dto.OrderItemSubtotal
            };
            _context.OrderItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderItem), new { id = item.OrderItemId }, dto);
        }

        //Delete: api/OrderItem/{id}
        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            var item = await _context.OrderItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            _context.OrderItems.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();

        }

    
    }

}



