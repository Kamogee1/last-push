using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace SingularSystems_SelfKiosk_Software.DTO
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(150, ErrorMessage = "Product Name cannot exceed 150 characters.")]
        public string ProductName { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }
        [Range(0.01, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description can't exceed 500 characters.")]
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        [JsonIgnore]
        public DateTime? LastUpdated { get; set; }
        [Required(ErrorMessage = "CategoryId is required.")]
        public int CategoryId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? ImageUrl { get; set; }
        public int? SupplierId { get; set; }

        // ✅ IFormFile — the image file being uploaded via the form
        [Required(ErrorMessage = "Image file is required.")]
        public IFormFile File { get; set; }
    }
}

