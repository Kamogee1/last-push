using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Identity.Client;
using Singular_Systems_SelfKiosk_Software.Models;
using SingularSystems_SelfKiosk_Software.Data;
using SingularSystems_SelfKiosk_Software.DTO;



namespace SingularSystems_SelfKiosk_Software.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly DataContext _context;

        public ProductController(DataContext context)
        {
            _context = context;
        }

        //GET: api/Product

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
                    ImageUrl = p.Image,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.CategoryName

                })

            .ToListAsync();

            return Ok(products);

        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await _context.Products
               .Include(p => p.Category)
               .Where(p => p.ProductId == id)
               .Select(p => new ProductDTO

               { 
                   ProductId = p.ProductId,
                   ProductName = p.ProductName,
                   Price = p.Price,
                   Quantity = p.Quantity,
                   IsAvailable = p.IsAvailable,
                   LastUpdated = p.LastUpdated,
                   CategoryId = p.CategoryId,
                   CategoryName = p.Category.CategoryName,

                   ImageUrl = string.IsNullOrEmpty(p.Image)
               ? null
               : $"{Request.Scheme}://{Request.Host}/{p.Image}"

               })

                 .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            return Ok(product);
        }


        //GET api/products/category/1?page=1&size=10
        [HttpGet("Category/{categoryId}")]

        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsByCategory(int categoryId, int page = 1, int size = 10)
        {

            if (page <= 0) page = 1;
            if (size <= 0 || size > 100) size = 10;

            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == categoryId)
                    .AsNoTracking()
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
                        ImageUrl = p.Image,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.CategoryName
                    })
                    .ToListAsync();


                if (!products.Any())
                {
                    return NotFound(new { message = "No products found for the specified category." });
                }
                return Ok(products);

            }
            catch (Exception ex)

            {
                Console.WriteLine($"Error fetching products : {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching products." });

            }
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult> CreateProduct([FromBody] ProductDTO dto)

        {
            if (string.IsNullOrEmpty(dto.Description))
            {
                return BadRequest("Description is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.ProductName))
                return BadRequest(new { message = "Product name is required." });

            if ((dto.Price <= 0))   
                return BadRequest(new { message = "Price must be greater than 0" });

            if (dto.Quantity < 0)
                return BadRequest(new { message = "Quantity cannot be negetive" });

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId);
            if(!categoryExists)
                return BadRequest(new { message = "Invalid CategeotyId" });

            var defaultSupplier = await _context.Suppliers.FirstOrDefaultAsync();
            if (defaultSupplier == null)
                return BadRequest("No supplier exists. Please add one before creating a product.");


            var product = new Product
            {
                ProductName = dto.ProductName,
                Price = dto.Price,
                Description = dto.Description,
                Quantity = dto.Quantity,
                IsAvailable = dto.IsAvailable,
                CategoryId = dto.CategoryId,
                SupplierId = dto.SupplierId ?? defaultSupplier.SupplierId,
                LastUpdated = DateTime.UtcNow,

                Image = null
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, new { message = "Product created successfully." });

        }

        // PUT: api/products/5
        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDTO dto) 
        {


            if (id != dto.ProductId)
                return BadRequest(new { message = "Product ID mismatch." });

            if (string.IsNullOrWhiteSpace(dto.ProductName))
                return BadRequest(new { message = "Product name is required." });

            if (dto.Price <= 0)
                return BadRequest(new { message = "Price must be greater than 0." });

            if (dto.Quantity < 0)
                return BadRequest(new { message = "Quantity cannot be negative." });

            if (string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest(new { message = "Product description is required." });

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId);
            if (!categoryExists)
                return BadRequest(new { message = "Invalid CategoryId." });

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return NotFound(new { message = "Product not found." });

            existingProduct.ProductName = dto.ProductName;
            existingProduct.Price = dto.Price;
            existingProduct.Quantity = dto.Quantity;
            existingProduct.IsAvailable = dto.IsAvailable;
            existingProduct.CategoryId = dto.CategoryId;
            existingProduct.Description = dto.Description;
            existingProduct.LastUpdated = DateTime.UtcNow;

            _context.Entry(existingProduct).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product updated successfully." });
        }


        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadProductImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
                return BadRequest("Only PNG and JPG files are allowed.");

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Product not found.");

            // Save file to wwwroot/images/products
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + ext;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Store absolute image URL
            var imageUrl = $"{Request.Scheme}://{Request.Host}/images/products/{uniqueFileName}";
            product.Image = imageUrl;

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Image uploaded successfully.",
                imageUrl = imageUrl
            });
        }


        // DELETE: api/product/5
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


        private async Task<string> UploadImageAsync(IFormFile file)
        {
            // 1. Generate a unique file name
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // 2. Choose a folder to save the image
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

            // 3. Make sure the folder exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // 4. Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 5. Return the relative path or full URL
            var imageUrl = $"/images/{fileName}"; // or full URL like $"https://localhost:5001/images/{fileName}"
            return imageUrl;
        }


    }












}



     
