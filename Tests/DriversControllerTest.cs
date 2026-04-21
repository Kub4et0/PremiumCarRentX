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
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rent_a_car.Tests
{
    [TestFixture]
    public class DriversControllerTests
    {
        private ApplicationDbContext _context;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private DriversController _controller;

        [SetUp]
        public void Setup()
        {
            // 1. Настройка на InMemory база данни
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "DriversTestDb_" + System.Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // 2. Mock-ване на UserManager
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

            // 3. Инициализация на контролера
            _controller = new DriversController(_context, _userManagerMock.Object);

            // 4. Настройка на TempData
            var httpContext = new DefaultHttpContext();
            _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewWithAllDrivers()
        {
            // Arrange
            _context.Drivers.AddRange(new List<Driver>
            {
                new Driver { Id = 1, FullName = "Иван Иванов" },
                new Driver { Id = 2, FullName = "Петър Петров" }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Index() as ViewResult;
            var model = result?.Model as List<Driver>;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(model.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task Create_Post_ValidModel_AddsDriverAndRedirects()
        {
            // Arrange
            var driver = new Driver { FullName = "Нов Шофьор", ExperienceYears = 5 };

            // Act
            var result = await _controller.Create(driver) as RedirectToActionResult;

            // Assert
            Assert.That(_context.Drivers.Count(), Is.EqualTo(1));
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(_controller.TempData["SuccessMessage"], Is.EqualTo("Шофьорът беше добавен успешно."));
        }

        [Test]
        public async Task DeleteConfirmed_RemovesDriverAndRelatedRentals()
        {
            // Arrange
            var driver = new Driver { Id = 5, FullName = "За Изтриване" };
            var rental = new Rental { Id = 1, DriverId = 5, Status = "Completed" };

            _context.Drivers.Add(driver);
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteConfirmed(5) as RedirectToActionResult;

            // Assert
            Assert.That(_context.Drivers.Count(), Is.EqualTo(0));
            Assert.That(_context.Rentals.Count(), Is.EqualTo(0)); // Тестваме каскадното изчистване
            Assert.That(result.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task AssignedRentals_ReturnsOnlyRentalsForLoggedInDriver()
        {
            // Arrange
            var user = new ApplicationUser { FullName = "Шофьор Тест" };
            var driver = new Driver { Id = 10, FullName = "Шофьор Тест" };
            var car = new Car { Id = 1, Brand = "Audi" };

            var rental1 = new Rental { Id = 1, DriverId = 10, Car = car }; // За този шофьор
            var rental2 = new Rental { Id = 2, DriverId = 99, Car = car }; // За друг шофьор

            _context.Drivers.Add(driver);
            _context.Cars.Add(car);
            _context.Rentals.AddRange(rental1, rental2);
            await _context.SaveChangesAsync();

            // Настройваме UserManager да връща нашия потребител
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            var result = await _controller.AssignedRentals() as ViewResult;
            var model = result?.Model as List<Rental>;

            // Assert
            Assert.That(model.Count, Is.EqualTo(1));
            Assert.That(model.First().DriverId, Is.EqualTo(10));
        }
    }
}