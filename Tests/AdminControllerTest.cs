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
using System.Threading.Tasks;

namespace Rent_a_car.Tests
{
    [TestFixture]
    public class AdminControllerTests
    {
        private ApplicationDbContext _context;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private AdminController _controller;

        [SetUp]
        public void Setup()
        {
            // 1. Настройка на InMemory база данни с уникално име за всеки тест
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Rent_a_carDb_" + System.Guid.NewGuid().ToString())
                .Options; // Коригирано от .Option на .Options

            _context = new ApplicationDbContext(options);

            // 2. Настройка на Mock за UserManager
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            // 3. Инициализация на контролера
            _controller = new AdminController(_context, _mockUserManager.Object);

            // 4. Инициализация на TempData (необходимо, за да работи DeleteMessage теста)
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new Mock<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void Dashboard_ReturnsViewWithLatestRentals()
        {
            // Arrange
            _context.Cars.Add(new Car { Id = 1, Brand = "Tesla", Model = "Model S", IsAvailable = true });
            _context.Rentals.Add(new Rental { Id = 1, Status = "Active", TotalPrice = 100 });
            _context.SaveChanges();

            // Act
            var result = _controller.Dashboard() as ViewResult;

            // Assert (Синтаксис за NUnit 4)
            Assert.That(result, Is.Not.Null);
            Assert.That(_controller.ViewBag.TotalCars, Is.EqualTo(1));
            Assert.That(result.Model, Is.InstanceOf<List<Rental>>());
        }

        [Test]
        public void Users_ReturnsViewWithUserList()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Email = "user1@test.com", UserName = "user1" },
                new ApplicationUser { Email = "user2@test.com", UserName = "user2" }
            }.AsQueryable();

            _mockUserManager.Setup(x => x.Users).Returns(users);

            // Act
            var result = _controller.Users() as ViewResult;
            var model = result?.Model as List<ApplicationUser>;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(model.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task DeleteMessage_RemovesMessageAndRedirects()
        {
            // Arrange - Увери се, че имената на свойствата съвпадат с твоя модел (напр. FullName)
            var message = new ContactMessage { Id = 1, Name = "Test User", Message = "Hello", SentAt = System.DateTime.Now };
            _context.ContactMessages.Add(message);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteMessage(1) as RedirectToActionResult;

            // Assert
            Assert.That(_context.ContactMessages.Count(), Is.EqualTo(0));
            Assert.That(result.ActionName, Is.EqualTo("Contacts"));
            Assert.That(_controller.TempData["SuccessMessage"], Is.EqualTo("Съобщението беше изтрито успешно."));
        }
    }
}