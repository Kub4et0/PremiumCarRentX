using System.ComponentModel.DataAnnotations;

namespace Rent_a_car.Models
{
    public class Driver
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Името на шофьора е задължително.")]
        [StringLength(100, ErrorMessage = "Името не може да бъде повече от 100 символа.")]
        [Display(Name = "Име и фамилия")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Телефонният номер е задължителен.")]
        [Phone(ErrorMessage = "Невалиден телефонен номер.")]
        [StringLength(20)]
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; }

        [Range(0, 50, ErrorMessage = "Опитът трябва да бъде между 0 и 50 години.")]
        [Display(Name = "Стаж (години)")]
        public int ExperienceYears { get; set; }


        [Display(Name = "Снимка (URL)")]
        public string? ImageUrl { get; set; } 

        [StringLength(500, ErrorMessage = "Биографията не може да бъде повече от 500 символа.")]
        [Display(Name = "Информация за шофьора")]
        public string? Biography { get; set; }

        [Display(Name = "Наличен")]
        public bool IsAvailable { get; set; } = true;

        public ICollection<Rental>? Rentals { get; set; }
    }
}