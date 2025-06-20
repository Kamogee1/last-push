using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Singular_Systems_SelfKiosk_Software.Models;
using SingularSystems_SelfKiosk_Software.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using SingularSystems_SelfKiosk_Software.DTO;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using BCrypt.Net;
using SingularSystems_SelfKiosk_Software.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SingularKiosk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserRegistrationDTO>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return users.Select(user => new UserRegistrationDTO
            {
                UserName = user.UserName,
                UserPassword = "[HIDDEN]",
                Name = user.Name,
                Surname = user.Surname,
                UserEmail = user.UserEmail
            }).ToList();
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserRegistrationDTO>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return new UserRegistrationDTO
            {
                UserName = user.UserName,
                UserPassword = "[HIDDEN]",
                Name = user.Name,
                Surname = user.Surname,
                UserEmail = user.UserEmail
            };
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<UserRegistrationDTO>> CreateUser(UserRegistrationDTO dto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == dto.UserEmail);
                if (existingUser != null)
                    return BadRequest("User already exists.");

                  // Validate email domain 
                if (!Regex.IsMatch(dto.UserEmail, @"^[^@\s]+@singular\.com$", RegexOptions.IgnoreCase))
                {
                    return BadRequest("Email must be in the format 'yourname@singular.com'.");
                }

                // Validate password complexity
                if (string.IsNullOrWhiteSpace(dto.UserPassword) ||
                    !Regex.IsMatch(dto.UserPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$"))
                {
                    return BadRequest("Password must be at least 8 characters long and contain uppercase, lowercase, number, and symbol.");
                }

                // Determine role based on email
                int roleId = dto.UserEmail.Contains("admin@singular.com", StringComparison.OrdinalIgnoreCase) ? 1 : 2;

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.UserPassword);

                // Create user
                var user = new User
                {
                    UserName = dto.UserName,
                    Name = dto.Name,
                    Surname = dto.Surname,
                    UserEmail = dto.UserEmail,
                    UserPassword = hashedPassword,
                    IsActive = true,
                    RoleId = roleId
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var wallet = new Wallet
                {
                    UserId = user.UserId,
                    Balance = 0.00m,
                    CustomerTransactions = new List<CustomerTransaction>()
                };

                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, new
                {
                    message = "Successfully created a profile.",
                    userId = user.UserId,
                    roleId = user.RoleId,
                    walletId = wallet
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating profile: {ex.Message}");
                return StatusCode(500, new { message = "Profile could not be created." });
            }
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserRegistrationDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!Regex.IsMatch(dto.UserEmail, @"^[^@\s]+@singular\.com$", RegexOptions.IgnoreCase))
            {
                return BadRequest("Email must be in the format 'yourname@singular.com'.");
            }

            user.RoleId = dto.UserEmail.Contains("admin@singular.com", StringComparison.OrdinalIgnoreCase) ? 1 : 2;

            user.UserName = dto.UserName;
            user.Name = dto.Name;
            user.Surname = dto.Surname;
            user.UserEmail = dto.UserEmail;

            if (!string.IsNullOrWhiteSpace(dto.UserPassword))
            {
                if (!Regex.IsMatch(dto.UserPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$"))
                {
                    return BadRequest("Password must be at least 8 characters long and contain uppercase, lowercase, number, and symbol.");
                }

                user.UserPassword = BCrypt.Net.BCrypt.HashPassword(dto.UserPassword);
            }

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Successfully deleted user." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user: {ex.Message}");
                return StatusCode(500, new { message = "User could not be deleted." });
            }
        }

        // ✅ POST: api/users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            Console.WriteLine($"Login attempt: Email={loginDto.userEmail}, Password={loginDto.password}");

            if (string.IsNullOrWhiteSpace(loginDto.userEmail) || string.IsNullOrWhiteSpace(loginDto.password))
                return BadRequest("Email and password are required.");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                Console.WriteLine("Validation Errors: " + string.Join(", ", errors));
                return BadRequest(errors);
            }

            // ✅ Load user including their Wallet
            var user = await _context.Users
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.UserEmail == loginDto.userEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.password, user.UserPassword))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var token = GenerateJwtToken(user);

            // Return user object with walletId inside user
            return Ok(new
            {
                message = "Login successful.",
                token = token,
                user = new
                {
                    userId = user.UserId,
                    name = user.Name,
                    email = user.UserEmail,
                    role = user.RoleId,
                    userRole = user.RoleId == 1 ? "admin" : "user",
                    walletId = user.Wallet?.Id 
                }
            });
        }

        // JWT Token Generator
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserEmail),
                new Claim("userId", user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.RoleId == 1 ? "admin" : "user"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

    }

