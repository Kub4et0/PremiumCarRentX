namespace Rent_a_car.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalCars { get; set; }

        public int AvailableCars { get; set; }

        public int TotalDrivers { get; set; }

        public int AvailableDrivers { get; set; }

        public int TotalRentals { get; set; }

        public int ActiveRentals { get; set; }

        public int PendingRentals { get; set; }

        public decimal TotalRevenue { get; set; }
    }
}
