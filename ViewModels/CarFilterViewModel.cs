namespace Rent_a_car.ViewModels
{
    public class CarFilterViewModel
    {
        public string? SearchString { get; set; }

        public string? Class { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public List<string>? Classes { get; set; }
    }
}
