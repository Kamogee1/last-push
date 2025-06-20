using System.ComponentModel.DataAnnotations;

namespace SingularSystems_SelfKiosk_Software.DTO
{
    public class CategoryDTO
    {
        public int CategoryId { get; set;  }


        [Required(ErrorMessage = "Category Name is required.")]
        [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters.")]
        public string CategoryName { get; set; }
    }
}
