using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SingularSystems_SelfKiosk_Software.DTO_s
{
    public class AddFundsDTO
    {

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]

        public decimal Amount { get; set; }
    }
}
