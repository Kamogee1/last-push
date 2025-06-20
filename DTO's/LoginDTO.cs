using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace SingularSystems_SelfKiosk_Software.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [JsonPropertyName("userEmail")]
        public string userEmail { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [JsonPropertyName("password")]
        public string password { get; set; }
    }
}
    



