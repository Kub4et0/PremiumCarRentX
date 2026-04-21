using Microsoft.AspNetCore.Http;
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
using System.Threading.Tasks;

namespace Rent_a_car.Tests
{
    [TestFixture]
    public class CarsControllerTests
    {
        private ApplicationDbContext _context;
        private CarsController _controller;

        [SetUp]
        public void Setup()
        {
            // Настройка на InMemory база данни
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CarsTestDb_" + System.Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            // Инициализация на контролера
            _controller = new CarsController(_context);

            // Настройка на TempData
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new Mock<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewWithFilteredCarsByClass()
        {
            // Arrange
            _context.Cars.AddRange(new List<Car>
            {
                new Car { Id = 1, Brand = "Audi", Model = "RS6", Class = "Luxury", Transmission = "Automatic", PricePerDay = 300 },
                new Car { Id = 2, Brand = "VW", Model = "Golf", Class = "Economy", Transmission = "Manual", PricePerDay = 50 }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Index(null, "Luxury", null, null, null, null) as ViewResult;
            var model = result?.Model as List<Car>;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(model.Count, Is.EqualTo(1));
            Assert.That(model.First().Brand, Is.EqualTo("Audi"));
        }

        [Test]
        public async Task Index_FiltersByMaxPrice()
        {
            // Arrange
            _context.Cars.AddRange(new List<Car>
            {
                new Car { Id = 1, Brand = "Tesla", PricePerDay = 200 },
                new Car { Id = 2, Brand = "Fiat", PricePerDay = 40 }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Index(null, null, null, 100, null, null) as ViewResult;
            var model = result?.Model as List<Car>;

            // Assert
            Assert.That(model.Count, Is.EqualTo(1));
            Assert.That(model.First().Brand, Is.EqualTo("Fiat"));
        }

        [Test]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var newCar = new Car { Brand = "BMW", Model = "M3", PricePerDay = 180 };

            // Act
            var result = await _controller.Create(newCar) as RedirectToActionResult;

            // Assert
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(_context.Cars.Count(), Is.EqualTo(1));
            Assert.That(_controller.TempData["SuccessMessage"], Is.EqualTo("Автомобилът беше добавен успешно."));
        }

        [Test]
        public async Task DeleteConfirmed_RemovesCarAndRelatedRentals()
        {
            // Arrange
            var car = new Car { Id = 10, Brand = "Mercedes" };
            var rental = new Rental { Id = 1, CarId = 10, Status = "Pending" };
            _context.Cars.Add(car);
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteConfirmed(10) as RedirectToActionResult;

            // Assert
            Assert.That(_context.Cars.Count(), Is.EqualTo(0));
            Assert.That(_context.Rentals.Count(), Is.EqualTo(0)); // Тестваме логиката за Foreign Key
            Assert.That(result.ActionName, Is.EqualTo("Index"));
        }
    }
}