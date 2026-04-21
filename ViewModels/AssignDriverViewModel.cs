using Microsoft.AspNetCore.Mvc.Rendering;

namespace Rent_a_car.ViewModels
{
    public class AssignDriverViewModel
    {
        public int RentalId { get; set; }

        public int? DriverId { get; set; }

        public IEnumerable<SelectListItem>? Drivers { get; set; }
    }
}
