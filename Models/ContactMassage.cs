using System.ComponentModel.DataAnnotations;

namespace Rent_a_car.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Името е задължително")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Имейлът е задължителен")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Темата е задължителна")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Съобщението не може да бъде празно")]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
