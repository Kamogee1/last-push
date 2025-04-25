using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SingularSystems_SelfKiosk_Software.Data;
using SingularSystems_SelfKiosk_Software.DTO;
using SingularSystems_SelfKiosk_Software.Models;

namespace SingularSystems_SelfKiosk_Software.Controllers.Properties
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly DataContext _context;

        public SupplierController(DataContext context)
        {
            _context = context;
        }


        // GET: api/Supplier
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplierDTO>>> GetSuppliers()
        {
            var suppliers = await _context.Suppliers
                .Select(s => new SupplierDTO
                {
                    SupplierId = s.SupplierId,
                    Name = s.Name,
                    Surname = s.Surname,
                    Email = s.Email
                })
                .ToListAsync();

            return Ok(suppliers);
        }

        // GET: api/Supplier/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SupplierDTO>> GetSupplier(int id)
        {
            var supplier = await _context.Suppliers
                .Where(s => s.SupplierId == id)
                .Select(s => new SupplierDTO
                {
                    SupplierId = s.SupplierId,
                    Name = s.Name,
                    Surname = s.Surname,
                    Email = s.Email
                })
                .FirstOrDefaultAsync();

            if (supplier == null)
                return NotFound();

            return Ok(supplier);
        }

        // POST: api/Supplier
        [HttpPost]
        public async Task<ActionResult<SupplierDTO>> CreateSupplier(SupplierDTO dto)
        {
            var supplier = new Supplier
            {
                Name = dto.Name,
                Surname = dto.Surname,
                Email = dto.Email
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            dto.SupplierId = supplier.SupplierId;

            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.SupplierId }, dto);
        }

        // PUT: api/Supplier/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, SupplierDTO dto)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();

            supplier.Name = dto.Name;
            supplier.Surname = dto.Surname;
            supplier.Email = dto.Email;

            _context.Entry(supplier).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Suppliers.Any(e => e.SupplierId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Supplier/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return NoContent();

        }
    }
}
