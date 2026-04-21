using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rent_a_car.Data;
using Rent_a_car.Models;

namespace Rent_a_car.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Dashboard()
        {
            ViewBag.TotalCars = _context.Cars.Count();
            ViewBag.AvailableCars = _context.Cars.Count(c => c.IsAvailable);
            ViewBag.TotalDrivers = _context.Drivers.Count();
            ViewBag.AvailableDrivers = _context.Drivers.Count(d => d.IsAvailable);
            ViewBag.TotalRentals = _context.Rentals.Count();
            ViewBag.ActiveRentals = _context.Rentals.Count(r => r.Status == "Active");
            ViewBag.PendingRentals = _context.Rentals.Count(r => r.Status == "Pending");
            ViewBag.TotalUsers = _userManager.Users.Count();

            ViewBag.TotalRevenue = _context.Rentals
                .Where(r => r.Status == "Completed")
                .Sum(r => (decimal?)r.TotalPrice) ?? 0;

            var latestRentals = _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .OrderByDescending(r => r.Id)
                .Take(5)
                .ToList();

            return View(latestRentals);
        }

        public IActionResult Users()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        public IActionResult Contacts()
        {
            var messages = _context.ContactMessages
                .OrderByDescending(m => m.SentAt) 
                .ToList();

            return View(messages);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                _context.ContactMessages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Съобщението беше изтрито успешно.";
            }
            return RedirectToAction(nameof(Contacts));
        }
    } 
}
