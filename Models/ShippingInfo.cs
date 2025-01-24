using System.ComponentModel.DataAnnotations;

namespace BookShopTest.Models
{
    public class ShippingInfo
    {
        [Display(Name = "Full Name")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full Name can only contain letters.")]
        public string FullName { get; set; }

        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Country can only contain letters.")]
        public string Country { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Postal Code can only contain numbers.")]
        public string PostalCode { get; set; }

        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone Number can only contain numbers.")]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }
}
