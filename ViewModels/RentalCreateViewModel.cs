using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Rent_a_car.ViewModels
{
    public class RentalCreateViewModel
    {
        public int CarId { get; set; }
        public string? CarName { get; set; }
        public decimal PricePerDay { get; set; }

        [Required(ErrorMessage = "Началната дата е задължителна")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Крайната дата е задължителна")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public bool WithDriver { get; set; }

        public int? DriverId { get; set; }
        public List<SelectListItem>? Drivers { get; set; }
    }
}
