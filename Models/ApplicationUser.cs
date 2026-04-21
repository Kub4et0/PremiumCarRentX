using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Rent_a_car.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string FullName { get; set; } = "";
    }
}
