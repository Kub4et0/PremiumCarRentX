using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rent_a_car.Models
{
    public class Car
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Марката е задължителна.")]
        [StringLength(50, ErrorMessage = "Марката не може да бъде повече от 50 символа.")]
        public string Brand { get; set; }

        [Required(ErrorMessage = "Моделът е задължителен.")]
        [StringLength(50, ErrorMessage = "Моделът не може да бъде повече от 50 символа.")]
        public string Model { get; set; }

        [Range(2000, 2100, ErrorMessage = "Годината трябва да бъде между 2000 и 2100.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Класът е задължителен.")]
        [StringLength(30, ErrorMessage = "Класът не може да бъде повече от 30 символа.")]
        public string Class { get; set; }

        [Required(ErrorMessage = "Цената е задължителна.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 10000, ErrorMessage = "Цената трябва да бъде между 0.01 и 10000.")]
        public decimal PricePerDay { get; set; }

        public string Transmission { get; set; }

        public bool IsAvailable { get; set; } = true;

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public ICollection<Rental>? Rentals { get; set; }
    }
}
