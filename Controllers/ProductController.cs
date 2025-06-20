using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Singular_Systems_SelfKiosk_Software.Models;
using SingularSystems_SelfKiosk_Software.Data;
using SingularSystems_SelfKiosk_Software.DTO;

namespace SingularSystems_SelfKiosk_Software.Controllers
{

    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly DataContext _context;

        public ProductController(DataContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAllProducts(int page = 1, int size = 10)
        {
            if (page <= 0) page = 1;
            if (size <= 0 || size > 100) size = 10;

            var products = await _context.Products
                .Include(p => p.Category)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    IsAvailable = p.IsAvailable,
                    LastUpdated = p.LastUpdated,
                    Description = p.Description,
                    ImageUrl = !string.IsNullOrEmpty(p.Image)
                        ? $"{Request.Scheme}://{Request.Host}/{p.Image.Replace("\\", "/")}"
                        : null,
                    CategoryId = p.CategoryId
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await _context.Products
                .Where(p => p.ProductId == id)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    IsAvailable = p.IsAvailable,
                    LastUpdated = p.LastUpdated,
                    Description = p.Description,
                    ImageUrl = !string.IsNullOrEmpty(p.Image)
                        ? $"{Request.Scheme}://{Request.Host}/{p.Image.Replace("\\", "/")}"
                        : null,
                    CategoryId = p.CategoryId
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound(new { message = "Product not found." });

            return Ok(product);
        }

        // POST: api/products
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult> CreateProduct([FromForm] ProductDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ProductName))
                return BadRequest(new { message = "Product name is required." });

            if (string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest(new { message = "Description is required." });

            if (dto.Price <= 0)
                return BadRequest(new { message = "Price must be greater than 0." });

            if (dto.Quantity < 0)
                return BadRequest(new { message = "Quantity cannot be negative." });

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId);
            if (!categoryExists)
                return BadRequest(new { message = "Invalid CategoryId." });

            // Handle image upload
            string imageUrl = null;
            if (dto.File != null && dto.File.Length > 0)
            {
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
                var ext = Path.GetExtension(dto.File.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                    return BadRequest("Only PNG and JPG files are allowed.");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + ext;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                imageUrl = $"images/products/{uniqueFileName}";
            }

            int supplierId = 1; // Assuming default supplier

            var product = new Product
            {
                ProductName = dto.ProductName,
                Price = dto.Price,
                Description = dto.Description,
                Quantity = dto.Quantity,
                IsAvailable = dto.IsAvailable,
                CategoryId = dto.CategoryId,
                SupplierId = supplierId,
                LastUpdated = DateTime.UtcNow,
                Image = imageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var response = new
            {
                productId = product.ProductId,
                productName = product.ProductName,
                description = product.Description,
                price = product.Price,
                quantity = product.Quantity,
                isAvailable = product.IsAvailable,
                categoryId = product.CategoryId,
                imageUrl = imageUrl != null ? $"{Request.Scheme}://{Request.Host}/{imageUrl}" : null,
                message = "Product created successfully."
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, response);

        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductUpdateDTO productDto)
        {
            var existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null)
                return NotFound(new { Message = "Product not found." });
            if (string.IsNullOrWhiteSpace(productDto.ProductName))
                return BadRequest(new { Message = "Product name is required." });

            if (productDto.Price <= 0)
                return BadRequest(new { Message = "Price must be greater than 0." });

            if (productDto.Quantity < 0)
                return BadRequest(new { Message = "Quantity cannot be negative." });

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == productDto.CategoryId);
            if (!categoryExists)
                return BadRequest(new { Message = "Invalid CategoryId." });

            // Update fields
            existingProduct.ProductName = productDto.ProductName;
            existingProduct.Description = productDto.Description;
            existingProduct.Price = productDto.Price;
            existingProduct.Quantity = productDto.Quantity;
            existingProduct.IsAvailable = productDto.Quantity > 0;
            existingProduct.CategoryId = productDto.CategoryId;
            existingProduct.SupplierId = productDto.SupplierId;
            existingProduct.LastUpdated = DateTime.UtcNow;

            if (productDto.File != null)
            {
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
                var ext = Path.GetExtension(productDto.File.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                    return BadRequest(new { Message = "Only PNG and JPG files are allowed." });

                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingProduct.Image))
                {
                    var oldImagePath = Path.Combine("wwwroot", existingProduct.Image);
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + ext;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await productDto.File.CopyToAsync(stream);
                }

                existingProduct.Image = $"images/products/{uniqueFileName}";
            }

            await _context.SaveChangesAsync();

            // Return updated product + success message
            return Ok(new
            {
                Message = "Product updated successfully.",
                Product = new

                {

                    existingProduct.ProductId,
                    existingProduct.ProductName,
                    existingProduct.Price,
                    existingProduct.Quantity,
                    existingProduct.Description,
                    existingProduct.IsAvailable,
                    existingProduct.CategoryId,
                    existingProduct.SupplierId,
                    ImageUrl = existingProduct.Image
                }
            });
        }


        // DELETE: api/products/{id}
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found." });

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Product deleted successfully.",
                    productId = product.ProductId,
                    productName = product.ProductName
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while deleting the product." });

            }
        }


        }
    }    