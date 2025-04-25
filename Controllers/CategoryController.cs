using System.Formats.Asn1;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Singular_Systems_SelfKiosk_Software.Models;
using SingularSystems_SelfKiosk_Software.Data;
using SingularSystems_SelfKiosk_Software.DTO;

namespace SingularSystems_SelfKiosk_Software.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CategoryController : ControllerBase
    {
        private readonly DataContext _context;

        public CategoryController(DataContext context)

        {
            _context = context;
        }

        // GET: api/category
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllCategories()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryDTO
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/category/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Select(c => new CategoryDTO
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName          
                })
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(category);
        }

        // POST: api/category
        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> CreateCategory([FromBody] CategoryDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new Category
            {
                CategoryName = dto.CategoryName
            };  

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId }, new CategoryDTO
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName
            });
        }

        // PUT: api/category/5
        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryDTO>> UpdateCategory(int id, [FromBody] CategoryDTO dto)
        {
            if (id != dto.CategoryId)
                return BadRequest(new { message = "Category ID mismatch." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
                return NotFound(new { message = "Category not found." });

            existingCategory.CategoryName = dto.CategoryName;
            _context.Entry(existingCategory).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new CategoryDTO
            {
                CategoryId = existingCategory.CategoryId,
                CategoryName = existingCategory.CategoryName
            });
        }

        // DELETE: api/category/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<CategoryDTO>> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                category.CategoryId,
                category.CategoryName
            });

        }


    }
}

    
