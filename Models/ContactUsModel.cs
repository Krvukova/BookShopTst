using System.ComponentModel.DataAnnotations;

namespace BookShopTest.Models
{
    public class ContactUsModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Message { get; set; }
    }
}