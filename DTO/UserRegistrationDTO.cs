using System.ComponentModel.DataAnnotations;

namespace SingularSystems_SelfKiosk_Software.DTO
{
    public class UserRegistrationDTO
    {
        [Required(ErrorMessage = "Username is required ")]
        //[MinLength(9, ErrorMessage = "Username must be at least 9 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required ")]
       // [MinLength(9, ErrorMessage = "Password must be at least 9 characters")]
        public string UserPassword { get; set; }

        [Required(ErrorMessage = "Name is required ")]
        //[MinLength(9, ErrorMessage = "Name must be at least 9 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Surname is required ")]
        //[MinLength(9, ErrorMessage = "Surname must be at least 9 characters")]
        public required string Surname { get; set; }

        [Required(ErrorMessage = "Email is required")]
        //[EmailAddress(ErrorMessage = "Invalid email format")]
        public string UserEmail { get; set; }

      
    }



}
