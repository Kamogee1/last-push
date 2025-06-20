using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SingularSystems_SelfKiosk_Software.Data;
using SingularSystems_SelfKiosk_Software.DTO_s;
using SingularSystems_SelfKiosk_Software.Models;

namespace SingularSystems_SelfKiosk_Software.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly DataContext _context;

        public WalletController(DataContext context)
        {
            _context = context;
        }

        //GET: api/Wallet
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WalletDTO>>> GetWallets()
        {
            var wallets = await _context.Wallets
                .Select(w => new WalletDTO

                {
                    Id = w.Id,
                    UserId = w.UserId,
                    Balance = w.Balance
                })
                  .ToListAsync();

            return Ok(wallets);
        }

        //GET: api/Wallet/5
        [HttpGet("{id}")]

        public async Task<ActionResult<WalletDTO>> GetWallet(int id)
        {
            var wallet = await _context.Wallets.FindAsync(id);

            if (wallet == null)
            { 
              return NotFound();

            }

            var dto = new WalletDTO
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Balance = wallet.Balance
            };

            return Ok(dto);

        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Wallet>> GetWalletByUserId(int userId)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                return NotFound("Wallet not found for the user.");
            }

            return Ok(wallet);
        }

        // POST: api/Wallet
        [HttpPost]
        public async Task<ActionResult<WalletDTO>> CreateWallet(CreateWalletDTO dto)
        {
            var wallet = new Wallet
            {
                UserId = dto.UserId,
                Balance = dto.Balance,
                CustomerTransactions = new List<CustomerTransaction>()
            };

            _context.Wallets.Add(wallet); // ✅ not 'Wallet.Add'
            await _context.SaveChangesAsync();

            var result = new WalletDTO
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Balance = wallet.Balance
            };

            return CreatedAtAction(nameof(GetWallet), new { id = wallet.Id }, result);
        }
        [HttpGet("user/name")]
        public async Task<IActionResult> GetWalletByName([FromQuery] string name, [FromQuery] string surname)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname))
                {
                    return BadRequest(new { message = "Name and surname are required." });
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Name == name && u.Surname == surname);

                if (user == null)
                {
                    return NotFound(new { message = "User not found with the provided name and surname." });
                }

                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == user.UserId);

                if (wallet == null)
                {
                    return NotFound(new { message = "Wallet not found for the user." });
                }

                return Ok(new
                {
                    walletId = wallet.Id,
                    userId = user.UserId,
                    name = user.Name,
                    surname = user.Surname,
                    balance = wallet.Balance
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR during GetWalletByName:");
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    message = "Internal Server Error",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }



        [HttpPost("{id}/addfunds")]
        public async Task<IActionResult> AddFunds(int id, [FromBody] AddFundsDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var wallet = await _context.Wallets.FindAsync(id);
                if (wallet == null)
                {
                    return NotFound(new { message = $"Wallet with ID {id} not found." });
                }

                wallet.Balance += dto.Amount;

                
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Funds deposited successfully.",
                    newBalance = wallet.Balance
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR during AddFunds:");
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    message = "Internal Server Error",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }
        

        [HttpPut("user/{userId}")]
        public async Task<IActionResult> UpdateWalletByUserId(int userId, [FromBody] CreateWalletDTO dto)
        {
            if (userId != dto.UserId)
                return BadRequest("User ID in path and body do not match.");

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
                return NotFound();

            wallet.Balance = dto.Balance;

            _context.Entry(wallet).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }


        //DELETE: api/Wallet/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWallet(int id)
        {

            var wallet = await _context.Wallets.FindAsync(id);
            if (wallet == null)
            {
                return NotFound();
            }
            _context.Wallets.Remove(wallet);
            await _context.SaveChangesAsync();

            return NoContent();
        }




    }





}
