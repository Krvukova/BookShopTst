using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BookShopTest.Models
{
    public class EditBookViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naslov je obavezan.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Autor je obavezan.")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Cijena je obavezna.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cijena mora biti veća od nule.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Žanr je obavezan.")]
        public string Genre { get; set; }

        public IFormFile CoverImage { get; set; }

        [Required(ErrorMessage = "Opis je obavezan.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Količina je obavezna.")]
        [Range(1, int.MaxValue, ErrorMessage = "Količina mora biti barem 1.")]
        public int Quantity { get; set; }

        public string ExistingCoverImageUrl { get; set; }
    }
}