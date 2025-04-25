using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SingularSystems_SelfKiosk_Software.Models;

namespace Singular_Systems_SelfKiosk_Software.Models
{
    public class Product
{
        public int ProductId { get; set; } // Primary Key

        [Required]
        public int CategoryId { get; set; } // Foreign Key into the Product Table 

        public  int? SupplierId { get; set; } // Foreign Key into the Product Table 
       
        [Required]
        [StringLength(100)]
        [Column("Name")]
        public string ProductName { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }

        public DateTime LastUpdated { get; set; }

        public string? Image { get; set; }

        // Navigation property
        public Category Category { get; set; }

        public Supplier? Supplier { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }

      





    }
}
