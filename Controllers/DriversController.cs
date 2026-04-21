using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rent_a_car.Data;
using Rent_a_car.Models;

namespace Rent_a_car.Controllers
{
    public class DriversController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DriversController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var drivers = await _context.Drivers.ToListAsync();
            return View(drivers);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Driver driver)
        {
            if (!ModelState.IsValid)
            {
                return View(driver);
            }

            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Шофьорът беше добавен успешно.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null) return NotFound();

            return View(driver);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,PhoneNumber,ExperienceYears,ImageUrl,IsAvailable")] Driver driver)
        {
            if (id != driver.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(driver);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Промените по профила на шофьора бяха запазени успешно!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriverExists(driver.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Ако има грешка във валидацията, връщаме потребителя към формата
            return View(driver);
        }

        // Помощен метод, който вероятно вече имаш най-отдолу в контролера
        private bool DriverExists(int id)
        {
            return _context.Drivers.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == id);
            if (driver == null) return NotFound();

            return View(driver);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driver = await _context.Drivers.FindAsync(id);

            if (driver != null)
            {
                // 1. Намираме всички резервации, свързани с този шофьор
                var relatedRentals = _context.Rentals.Where(r => r.DriverId == id);

                // 2. Изтриваме първо тях (изчистваме историята)
                _context.Rentals.RemoveRange(relatedRentals);

                // 3. Сега вече можем безопасно да изтрием и шофьора
                _context.Drivers.Remove(driver);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Шофьорът и неговата история бяха изтрити успешно.";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> AssignedRentals()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.FullName == user.FullName);
            if (driver == null)
            {
                TempData["ErrorMessage"] = "Няма намерен профил на шофьор.";
                return RedirectToAction("Index", "Home");
            }

            var rentals = await _context.Rentals
                .Where(r => r.DriverId == driver.Id)
                .Include(r => r.Car)
                .Include(r => r.User)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(rentals);
        }
    }
}
