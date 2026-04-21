using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rent_a_car.Data;
using Rent_a_car.Models;
using System.Linq;

namespace Rent_a_car.Controllers
{
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cars
        public async Task<IActionResult> Index(string searchString, string carClass, string transmission, decimal? maxPrice, int? year, string sortOrder)
        {
            ViewBag.Classes = await _context.Cars
                .Select(c => c.Class)
                .Distinct()
                .Where(c => c != null)
                .OrderBy(c => c)
                .ToListAsync();

            var cars = _context.Cars.AsQueryable();

            if (!string.IsNullOrEmpty(carClass))
                cars = cars.Where(c => c.Class == carClass);

            if (!string.IsNullOrEmpty(searchString))
                cars = cars.Where(c => c.Brand.Contains(searchString) || c.Model.Contains(searchString));

            if (!string.IsNullOrEmpty(transmission))
                cars = cars.Where(c => c.Transmission == transmission);

            if (maxPrice.HasValue)
                cars = cars.Where(c => c.PricePerDay <= maxPrice.Value);

            if (year.HasValue)
                cars = cars.Where(c => c.Year >= year.Value);

            cars = sortOrder switch
            {
                "priceDesc" => cars.OrderByDescending(c => c.PricePerDay),
                "priceAsc" => cars.OrderBy(c => c.PricePerDay),
                _ => cars.OrderBy(c => c.Brand)
            };

            return View(await cars.ToListAsync());
        }

        // GET: Cars/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars.FirstOrDefaultAsync(m => m.Id == id);
            if (car == null) return NotFound();

            return View(car);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car car)
        {
            if (ModelState.IsValid)
            {
                _context.Cars.Add(car);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Автомобилът беше добавен успешно.";
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();
            return View(car);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Brand,Model,Year,Class,Transmission,PricePerDay,ImageUrl,IsAvailable")] Car car)
        {
            if (id != car.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Промените бяха запазени успешно!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.Id)) return NotFound();
                    else throw;
                }
            }

            // АКО СТИГНЕ ДОТУК, ЗНАЧИ ИМА ГРЕШКА ВЪВ ВАЛИДАЦИЯТА
            // Вземаме всички грешки и ги записваме в TempData, за да ги видиш на екрана
            var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["ErrorMessage"] = "Грешка при запис: " + string.Join(" | ", errorList);

            return View(car);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);
            if (car == null) return NotFound();
            return View(car);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                // Изтриване на свързаните резервации, за да няма грешка в базата (Foreign Key Constraint)
                var relatedRentals = _context.Rentals.Where(r => r.CarId == id);
                _context.Rentals.RemoveRange(relatedRentals);

                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Автомобилът беше премахнат.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id) => _context.Cars.Any(e => e.Id == id);
    }
}