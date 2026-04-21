using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Rent_a_car.Data;
using Rent_a_car.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rent_a_car.Tests
{
    [TestFixture]
    public class DbSeederTests
    {
        private Mock<IServiceProvider> _serviceProviderMock;
        private Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;

        [SetUp]
        public void Setup()
        {
            // 1. Mock-ване на RoleManager
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null);

            // 2. Mock-ване на UserManager
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            // 3. Mock-ване на IServiceProvider
            _serviceProviderMock = new Mock<IServiceProvider>();

            // Конфигуриране на доставчика да връща нашите макети
            _serviceProviderMock
                .Setup(sp => sp.GetService(typeof(RoleManager<IdentityRole>)))
                .Returns(_roleManagerMock.Object);

            _serviceProviderMock
                .Setup(sp => sp.GetService(typeof(UserManager<ApplicationUser>)))
                .Returns(_userManagerMock.Object);
        }

        [Test]
        public async Task SeedRolesAndAdminAsync_ShouldCreateRolesAndAdmin_WhenTheyDoNotExist()
        {
            // Arrange
            // Всички роли не съществуват
            _roleManagerMock.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _roleManagerMock.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

            // Администраторът не съществува
            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);
            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            await DbSeeder.SeedRolesAndAdminAsync(_serviceProviderMock.Object);

            // Assert
            // Проверка дали са извикани методите за създаване на трите роли
            _roleManagerMock.Verify(rm => rm.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Admin")), Times.Once);
            _roleManagerMock.Verify(rm => rm.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Client")), Times.Once);
            _roleManagerMock.Verify(rm => rm.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Driver")), Times.Once);

            // Проверка дали е създаден администраторът
            _userManagerMock.Verify(um => um.CreateAsync(It.Is<ApplicationUser>(u => u.Email == "admin@rentacar.com"), "Admin123!"), Times.Once);

            // Проверка дали администраторът е добавен към ролята Admin
            _userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"), Times.Once);
        }

        [Test]
        public async Task SeedRolesAndAdminAsync_ShouldNotCreateAnything_WhenTheyAlreadyExist()
        {
            // Arrange
            // Ролите вече съществуват
            _roleManagerMock.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            // Администраторът вече съществува
            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser());

            // Act
            await DbSeeder.SeedRolesAndAdminAsync(_serviceProviderMock.Object);

            // Assert
            // Методите за създаване НЕ трябва да бъдат извиквани
            _roleManagerMock.Verify(rm => rm.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
            _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }
    }
}