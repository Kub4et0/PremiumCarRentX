using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rent_a_car.Models
{
    public class Rental
    {
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }

        public Car? Car { get; set; }

        public int? DriverId { get; set; }

        public Driver? Driver { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Началната дата е задължителна.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Крайната дата е задължителна.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public bool WithDriver { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 100000, ErrorMessage = "Общата цена трябва да бъде положително число.")]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";
    }
}
