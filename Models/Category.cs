using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Singular_Systems_SelfKiosk_Software.Models
{
    public class Category
    {
       
        public int CategoryId { get; set; } //Primary Key


        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        public string CategoryName { get; set; }

        public  ICollection<Product> Products
        {
            get; set;

        }
    }
}
