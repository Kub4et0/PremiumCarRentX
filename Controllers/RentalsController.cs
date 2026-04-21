using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rent_a_car.Data;
using Rent_a_car.Models;
using Rent_a_car.ViewModels;

namespace Rent_a_car.Controllers
{
    [Authorize]
    public class RentalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public RentalsController(ApplicationDbContext context,
                                 UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int carId)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == carId);
            if (car == null) return NotFound();

            var model = new RentalCreateViewModel
            {
                CarId = car.Id,
                CarName = $"{car.Brand} {car.Model}",
                PricePerDay = car.PricePerDay,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                Drivers = await _context.Drivers
                    .Where(d => d.IsAvailable)
                    .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.FullName })
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentalCreateViewModel model)
        {
            var car = await _context.Cars.FindAsync(model.CarId);
            if (car == null) return NotFound();

            if (model.EndDate <= model.StartDate)
                ModelState.AddModelError("", "Крайната дата трябва да е след началната.");

            if (model.StartDate < DateTime.Today)
                ModelState.AddModelError("", "Не можете да наемате за минали дати.");

            bool isCarTaken = await _context.Rentals.AnyAsync(r =>
                r.CarId == model.CarId &&
                r.Status != "Cancelled" &&
                r.Status != "Completed" &&
                model.StartDate < r.EndDate && r.StartDate < model.EndDate);

            if (isCarTaken)
                ModelState.AddModelError("", "Автомобилът вече е резервиран за този период.");

            if (model.WithDriver && !model.DriverId.HasValue)
            {
                ModelState.AddModelError("", "Моля, изберете шофьор от списъка.");
            }

            if (!ModelState.IsValid)
            {
                model.CarName = $"{car.Brand} {car.Model}";
                model.PricePerDay = car.PricePerDay;
                model.Drivers = await _context.Drivers
                    .Where(d => d.IsAvailable)
                    .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.FullName })
                    .ToListAsync();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!User.IsInRole("Client") && !User.IsInRole("Admin") && !User.IsInRole("Driver"))
            {
                await _userManager.AddToRoleAsync(user, "Client");
                await _signInManager.RefreshSignInAsync(user);
            }

            int days = (model.EndDate - model.StartDate).Days;
            if (days <= 0) days = 1;
            decimal total = days * (car.PricePerDay + (model.WithDriver ? 50m : 0m));

            var rental = new Rental
            {
                CarId = model.CarId,
                UserId = user.Id,
                DriverId = model.WithDriver ? model.DriverId : null,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                WithDriver = model.WithDriver,
                TotalPrice = total,
                Status = "Pending"
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Резервацията е изпратена успешно!";
            return RedirectToAction(nameof(MyRentals));
        }

        [Authorize] 
        public async Task<IActionResult> MyRentals()
        {
            var user = await _userManager.GetUserAsync(User);
            var rentals = await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Driver)
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(rentals);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> All()
        {
            var allRentals = await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Driver)
                .Include(r => r.User)
                .OrderByDescending(r => r.Id)
                .ToListAsync();
            return View(allRentals);
        }

        [Authorize(Roles = "Admin,Driver")]
        [HttpPost]
        public async Task<IActionResult> EditStatus(int id, string status)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            rental.Status = status;
            await _context.SaveChangesAsync();

            if (User.IsInRole("Driver"))
            {
                return RedirectToAction("Index", "Drivers");
            }

            return RedirectToAction(nameof(All));
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageRentals()
        {
            var allRentals = await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.User) 
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            return View(allRentals);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ApproveRental(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            rental.Status = "Confirmed"; 
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Резервацията беше одобрена!";
            return RedirectToAction(nameof(ManageRentals));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RejectRental(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            rental.Status = "Rejected";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Резервацията беше отхвърлена.";
            return RedirectToAction(nameof(ManageRentals));
        }
    }
}