using System.ComponentModel.DataAnnotations;

namespace ZenithPM.Web.Models.ViewModels
{
    public class MfaViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        public string OTPCode { get; set; } = string.Empty;
    }
}