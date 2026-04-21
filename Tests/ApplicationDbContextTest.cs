using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Rent_a_car.Data;
using Rent_a_car.Models;
using System;
using System.Linq;

namespace Rent_a_car.Tests
{
    [TestFixture]
    public class ApplicationDbContextTests
    {
        private ApplicationDbContext _context;

        [SetUp]
        public void Setup()
        {
            // Настройка на база данни в паметта с уникално име за всяко тестване
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "RentACar_DB_Test_" + Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void VerifyCarsTable_CanInsertAndRetrieveData()
        {
            // Arrange
            var car = new Car
            {
                Brand = "BMW",
                Model = "M5",
                PricePerDay = 150,
                IsAvailable = true
            };

            // Act
            _context.Cars.Add(car);
            _context.SaveChanges();

            // Assert
            var retrievedCar = _context.Cars.FirstOrDefault(c => c.Brand == "BMW");
            Assert.That(retrievedCar, Is.Not.Null);
            Assert.That(retrievedCar.Model, Is.EqualTo("M5"));
        }

        [Test]
        public void VerifyRentalsRelationships_CanLinkCarAndUser()
        {
            // Arrange
            var car = new Car { Id = 1, Brand = "Audi", Model = "A6" };
            var user = new ApplicationUser { Id = "user-1", UserName = "testuser", FullName = "Test User" };
            var rental = new Rental
            {
                CarId = 1,
                UserId = "user-1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(3),
                Status = "Pending"
            };

            // Act
            _context.Cars.Add(car);
            _context.Users.Add(user);
            _context.Rentals.Add(rental);
            _context.SaveChanges();

            // Assert
            var savedRental = _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .FirstOrDefault();

            Assert.That(savedRental, Is.Not.Null);
            Assert.That(savedRental.Car.Brand, Is.EqualTo("Audi"));
            Assert.That(savedRental.User.FullName, Is.EqualTo("Test User"));
        }

        [Test]
        public void VerifyContactMessages_CanInsertMessage()
        {
            // Arrange
            var message = new ContactMessage
            {
                Name = "Иван Иванов",
                Email = "ivan@test.com",
                Subject = "Въпрос",
                Message = "Здравейте, имам въпрос.",
                SentAt = DateTime.Now
            };

            // Act
            _context.ContactMessages.Add(message);
            _context.SaveChanges();

            // Assert
            Assert.That(_context.ContactMessages.Count(), Is.EqualTo(1));
            Assert.That(_context.ContactMessages.First().Name, Is.EqualTo("Иван Иванов"));
        }
    }
}