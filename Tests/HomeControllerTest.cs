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
    public class HomeControllerTests
    {
        private ApplicationDbContext _context;
        private HomeController _controller;

        [SetUp]
        public void Setup()
        {
            // 1. Настройка на InMemory база данни
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "HomeTestDb_" + System.Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // 2. Инициализация на контролера
            _controller = new HomeController(_context);

            // 3. Настройка на TempData (за теста на контактната форма)
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
        public void Index_ReturnsTop6AvailableCars_OrderedByPrice()
        {
            // Arrange: Добавяме 8 коли (7 налични, 1 не)
            for (int i = 1; i <= 7; i++)
            {
                _context.Cars.Add(new Car { Id = i, Brand = "Car" + i, PricePerDay = 100 - i, IsAvailable = true });
            }
            _context.Cars.Add(new Car { Id = 8, Brand = "Unavailable", IsAvailable = false });
            _context.SaveChanges();

            // Act
            var result = _controller.Index() as ViewResult;
            var model = result?.Model as List<Car>;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(model.Count, Is.EqualTo(6)); // Трябва да вземе точно 6
            Assert.That(model.First().PricePerDay, Is.LessThan(model.Last().PricePerDay)); // Проверка на сортировката
            Assert.That(model.Any(c => !c.IsAvailable), Is.False); // Само налични
        }

        [Test]
        public void About_ReturnsViewResult()
        {
            // Act
            var result = _controller.About();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Contact_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Contact();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Contact_Post_ValidModel_SavesMessageAndRedirects()
        {
            // Arrange
            var message = new ContactMessage
            {
                Name = "Иван Иванов",
                Email = "ivan@test.com",
                Subject = "Въпрос",
                Message = "Тест съобщение"
            };

            // Act
            var result = await _controller.Contact(message) as RedirectToActionResult;

            // Assert
            Assert.That(_context.ContactMessages.Count(), Is.EqualTo(1)); // Запис в базата
            Assert.That(result.ActionName, Is.EqualTo("Contact")); // Редирект
            Assert.That(_controller.TempData["SuccessMessage"], Is.Not.Null); // TempData съобщение
        }

        [Test]
        public async Task Contact_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var message = new ContactMessage { Name = "" }; // Невалиден модел
            _controller.ModelState.AddModelError("FullName", "Required");

            // Act
            var result = await _controller.Contact(message) as ViewResult;

            // Assert
            Assert.That(_context.ContactMessages.Count(), Is.EqualTo(0)); // Не трябва да записва в DB
            Assert.That(result.Model, Is.EqualTo(message)); // Връща същия модел
        }
    }
}