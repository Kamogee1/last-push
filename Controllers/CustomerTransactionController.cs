using System.Formats.Asn1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Singular_Systems_SelfKiosk_Software.Models;
using SingularSystems_SelfKiosk_Software.Data;
using SingularSystems_SelfKiosk_Software.DTO;
using SingularSystems_SelfKiosk_Software.DTO_s;
using SingularSystems_SelfKiosk_Software.Models;

namespace SingularSystems_SelfKiosk_Software.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerTransactionController : ControllerBase
    {
        private readonly DataContext _context;

        public CustomerTransactionController(DataContext context)
        {
            _context = context;
        }

        // GET: api/CustomerTransaction
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerTransactionDTO>>> GetTransactions()
        {
            var transactions = await _context.CustomerTransactions
                .Select(t => new CustomerTransactionDTO
                {
                    CustomerTransactionId = t.CustomerTransactionId,
                    OrderId = t.OrderId,
                    WalletId = t.WalletId,
                    Amount = t.Amount,
                    Type = t.Type,
                    Date = t.Date,
                    Status = t.Status
                })
                .ToListAsync();

            return Ok(transactions);

        }
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerTransactionDTO>> GetTransaction(int id)
        {
            var t = await _context.CustomerTransactions.FindAsync(id);

            if (t == null)
                return NotFound();

            var dto = new CustomerTransactionDTO
            {
                CustomerTransactionId = t.CustomerTransactionId,
                OrderId = t.OrderId,
                WalletId = t.WalletId,
                Amount = t.Amount,
                Type = t.Type,
                Date = t.Date,
                Status = t.Status
            };

            return Ok(dto);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserTransactions(int userId)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
                return NotFound("Wallet not found");

            var transactions = await _context.CustomerTransactions
                .Include(t => t.Order)
                .Where(t => t.Wallet.UserId == userId)
                .OrderByDescending(t => t.Date)
                .Select(t => new
                {
                    t.CustomerTransactionId,
                    t.Type,
                    t.Amount,
                    t.Date,
                    t.Status,
                    Order = t.Order != null ? new
                    {
                        t.Order.OrderId,
                        t.Order.OrderTotalAmount,
                        t.Order.OrderDate
                    } : null
                })
                .ToListAsync();

            return Ok(transactions);
        }

        [HttpGet("wallet/{walletId}/transactions")]
        public async Task<ActionResult<IEnumerable<CustomerTransactionDTO>>> GetTransactionsByWallet(int walletId)
        {
            var transactions = await _context.CustomerTransactions
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.Date)
                .Select(t => new CustomerTransactionDTO
                {
                    CustomerTransactionId = t.CustomerTransactionId,
                    Amount = t.Amount,
                    Type = t.Type,
                    Date = t.Date,
                    Status = t.Status,
                    OrderId = t.OrderId
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // POST: api/CustomerTransaction
        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<CustomerTransactionDTO>> CreateTransaction(CustomerTransactionDTO dto)
        {
            Order order;

            if (dto.Type != "TopUp")
            {
                // Create real order for non-TopUp transactions
                order = new Order
                {
                    UserId = dto.UserId,
                    OrderDate = DateTime.Now,
                    OrderTotalAmount = dto.Amount,
                    OrderStatus = false,
                };
            }
            else
            {
                // For TopUp, create a dummy order to satisfy NOT NULL constraint (if needed)
                order = new Order
                {
                    UserId = dto.UserId,
                    OrderDate = DateTime.Now,
                    OrderTotalAmount = dto.Amount,
                    OrderStatus = true,  // or some flag to mark dummy orders
                };
            }
            var transaction = new CustomerTransaction
            {
                WalletId = dto.WalletId,
                Amount = dto.Amount,
                Type = dto.Type,
                Date = dto.Date,
                Status = dto.Status,
                Order = order  // EF Core will link OrderId automatically
            };

            _context.CustomerTransactions.Add(transaction);

            // Update wallet balance only for TopUp transactions
            if (dto.Type == "TopUp")
            {
                var wallet = await _context.Wallets.FindAsync(dto.WalletId);
                if (wallet == null)
                    return NotFound("Wallet not found");

                wallet.Balance += dto.Amount;
                _context.Wallets.Update(wallet);
            }

            await _context.SaveChangesAsync();

            dto.CustomerTransactionId = transaction.CustomerTransactionId;

            return CreatedAtAction(nameof(GetTransaction), new { id = dto.CustomerTransactionId }, dto);
        }

        // DELETE: api/CustomerTransaction/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.CustomerTransactions.FindAsync(id);
            if (transaction == null)
                return NotFound();

            _context.CustomerTransactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}







