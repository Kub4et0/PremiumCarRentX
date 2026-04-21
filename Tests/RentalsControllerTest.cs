using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Rent_a_car.Controllers;
using Rent_a_car.Data;
using Rent_a_car.Models;
using Rent_a_car.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rent_a_car.Tests
{
    [TestFixture]
    public class RentalsControllerTests
    {
        private ApplicationDbContext _context;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private RentalsController _controller;

        [SetUp]
        public void Setup()
        {
            // 1. Настройка на InMemory Database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "RentalsTestDb_" + Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // 2. Mock-ване на UserManager
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            // 3. Mock-ване на SignInManager
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object, contextAccessor.Object, claimsFactory.Object, null, null, null, null);

            // 4. Инициализация на контролера
            _controller = new RentalsController(_context, _mockUserManager.Object, _mockSignInManager.Object);

            // 5. Настройка на TempData
            var tempDataProvider = new Mock<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), tempDataProvider.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Create_Get_ReturnsCorrectViewModel()
        {
            // Arrange
            var car = new Car { Id = 1, Brand = "Mercedes", Model = "S-Class", PricePerDay = 200, IsAvailable = true };
            _context.Cars.Add(car);
            _context.Drivers.Add(new Driver { Id = 1, FullName = "Иван", IsAvailable = true });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Create(1) as ViewResult;
            var model = result?.Model as RentalCreateViewModel;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(model.CarId, Is.EqualTo(1));
            Assert.That(model.PricePerDay, Is.EqualTo(200));
            Assert.That(model.Drivers.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Create_Post_CalculatesCorrectTotalPrice()
        {
            // Arrange
            var car = new Car { Id = 1, PricePerDay = 100, IsAvailable = true };
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            var user = new ApplicationUser { Id = "user1", Email = "test@test.com" };
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var model = new RentalCreateViewModel
            {
                CarId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(3), // 3 дни
                WithDriver = true // +50 на ден
            };

            // Act
            await _controller.Create(model);

            // Assert
            var rental = _context.Rentals.First();
            // 3 дни * (100 + 50) = 450
            Assert.That(rental.TotalPrice, Is.EqualTo(450m));
            Assert.That(rental.Status, Is.EqualTo("Pending"));
        }

        [Test]
        public async Task Create_Post_Fails_IfDatesOverlap()
        {
            // Arrange
            var carId = 1;
            _context.Cars.Add(new Car { Id = carId, PricePerDay = 100 });
            _context.Rentals.Add(new Rental
            {
                CarId = carId,
                StartDate = DateTime.Today.AddDays(5),
                EndDate = DateTime.Today.AddDays(10),
                Status = "Confirmed"
            });
            await _context.SaveChangesAsync();

            var model = new RentalCreateViewModel
            {
                CarId = carId,
                StartDate = DateTime.Today.AddDays(7), // Застъпва се
                EndDate = DateTime.Today.AddDays(12)
            };

            // Act
            var result = await _controller.Create(model) as ViewResult;

            // Assert
            Assert.That(_controller.ModelState.IsValid, Is.False);
            Assert.That(_context.Rentals.Count(), Is.EqualTo(1)); // Не трябва да се добавя нов
        }

        [Test]
        public async Task ApproveRental_UpdatesStatusToConfirmed()
        {
            // Arrange
            var rental = new Rental { Id = 1, Status = "Pending" };
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.ApproveRental(1) as RedirectToActionResult;

            // Assert
            var updatedRental = await _context.Rentals.FindAsync(1);
            Assert.That(updatedRental.Status, Is.EqualTo("Confirmed"));
            Assert.That(_controller.TempData["SuccessMessage"], Is.EqualTo("Резервацията беше одобрена!"));
        }
    }
}