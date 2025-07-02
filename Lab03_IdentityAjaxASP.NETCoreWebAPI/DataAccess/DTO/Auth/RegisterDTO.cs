using System.ComponentModel.DataAnnotations;
using DataAccess.Constant.Enum;

namespace DataAccess.DTO.Auth
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Account Name must be between 2 and 100 characters")]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; } = (int)RoleEnum.Customer; // Default to Customer role
    }
}
