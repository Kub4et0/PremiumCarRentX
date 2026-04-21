using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rent_a_car.Data;
using Rent_a_car.Models;

namespace Rent_a_car.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var promoCars = _context.Cars
                .Where(c => c.IsAvailable)
                .OrderBy(c => c.PricePerDay)
                .Take(6)
                .ToList();

            return View(promoCars);
        }

        public IActionResult About()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMessage model)
        {
            if (ModelState.IsValid)
            {
                _context.ContactMessages.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Благодарим ви! Вашето съобщение беше изпратено успешно.";
                return RedirectToAction(nameof(Contact));
            }
            return View(model);
        }
    }
}

