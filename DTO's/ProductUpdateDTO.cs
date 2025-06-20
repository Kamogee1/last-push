using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SingularSystems_SelfKiosk_Software.DTO
{
    public class ProductUpdateDTO
    {
        public int ProductId { get; set; }
        [Required]
        [StringLength(150)]
        public string ProductName { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        [Range(0.01, int.MaxValue)]
        public int Quantity { get; set; }
        [Required]
        [StringLength(500)]
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        public int CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public int? SupplierId { get; set; }

        // Optional for updates
        [FromForm(Name = "File")]
        public IFormFile? File { get; set; }
    }
}
