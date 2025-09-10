using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirportBooking.Models;
using AirportBooking.Repositories;
using AirportBooking.Services;
using Moq;
using Xunit;

namespace AirportBookingTest
{
    [Trait("Category", "PassengerService")]
    public class PassengerServiceTests
    {
        private readonly Mock<IPassengerRepository> _mockPassengerRepo;
        private readonly PassengerService _passengerService;

        public PassengerServiceTests()
        {
            _mockPassengerRepo = new Mock<IPassengerRepository>();
            _passengerService = new PassengerService(_mockPassengerRepo.Object);
        }

        [Fact]
        public async Task GetPassengerByEmailAsync_ReturnsPassenger_WhenFound()
        {
            // Arrange
            var passenger = new Passenger { PassengerId = "P1", Email = "mays@test.com", FirstName = "Mays" };
            _mockPassengerRepo.Setup(r => r.GetByEmailAsync("mays@test.com"))
                .ReturnsAsync(passenger);

            // Act
            var result = await _passengerService.GetPassengerByEmailAsync("mays@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P1", result.PassengerId);
            Assert.Equal("Mays", result.FirstName);
        }

        [Fact]
        public async Task AddPassengerAsync_ThrowsException_WhenEmailMissing()
        {
            // Arrange
            var passenger = new Passenger { PassengerId = "P1", FirstName = "Mays", Email = "" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _passengerService.AddPassengerAsync(passenger));
        }

        [Fact]
        public async Task GetOrRegisterPassengerAsync_ReturnsExistingPassenger_WhenFound()
        {
            // Arrange
            var passenger = new Passenger { PassengerId = "P1", Email = "ahmad@test.com", FirstName = "Ahmad" };
            _mockPassengerRepo.Setup(r => r.GetByEmailAsync("ahmad@test.com"))
                .ReturnsAsync(passenger);

            // Act
            var result = await _passengerService.GetOrRegisterPassengerAsync("ahmad@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P1", result.PassengerId);
        }

        [Fact]
        public async Task GetOrRegisterPassengerAsync_RegistersNewPassenger_WhenNotFound()
        {
            // Arrange
            _mockPassengerRepo.Setup(r => r.GetByEmailAsync("ola@test.com"))
                .ReturnsAsync((Passenger?)null);

            Passenger? addedPassenger = null;
            _mockPassengerRepo.Setup(r => r.AddAsync(It.IsAny<Passenger>()))
                .Callback<Passenger>(p => addedPassenger = p)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _passengerService.GetOrRegisterPassengerAsync("ola@test.com", "Ola", "0599999999");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Ola", result.FirstName);
            Assert.Equal("ola@test.com", result.Email);
            Assert.Equal(addedPassenger?.PassengerId, result.PassengerId);
        }

        [Fact]
        public async Task GetOrRegisterPassengerAsync_ThrowsException_WhenNotFound_AndNoDetails()
        {
            // Arrange
            _mockPassengerRepo.Setup(r => r.GetByEmailAsync("nodetails@mail.com"))
                .ReturnsAsync((Passenger?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _passengerService.GetOrRegisterPassengerAsync("nodetails@mail.com"));
        }

         [Fact]
        public async Task UpdatePassengerAsync_CallsRepositoryUpdate()
        {
            // Arrange
            var passenger = new Passenger { PassengerId = "P1", Email = "mays@test.com", FirstName = "Mays" };
            _mockPassengerRepo.Setup(r => r.UpdateAsync(passenger))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _passengerService.UpdatePassengerAsync(passenger);

            // Assert
            _mockPassengerRepo.Verify(r => r.UpdateAsync(passenger), Times.Once);
        }

        [Fact]
        public async Task DeletePassengerAsync_CallsRepositoryDelete()
        {
            // Arrange
            var passengerId = "P1";
            _mockPassengerRepo.Setup(r => r.DeleteAsync(passengerId))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _passengerService.DeletePassengerAsync(passengerId);

            // Assert
            _mockPassengerRepo.Verify(r => r.DeleteAsync(passengerId), Times.Once);
        }
    }
}