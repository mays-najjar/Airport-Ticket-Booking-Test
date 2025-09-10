using Xunit;
using Moq;
using AirportBooking.Services;
using AirportBooking.Repositories;
using AirportBooking.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AirportBooking.Tests
{
    [Trait("Category", "FlightService")]
    public class FlightServiceTests
    {
        private readonly Mock<IFlightRepository> _mockFlightRepo;
        private readonly FlightService _flightService;

        public FlightServiceTests()
        {
            _mockFlightRepo = new Mock<IFlightRepository>();
            _flightService = new FlightService(_mockFlightRepo.Object);
        }

        [Fact]
        public async Task GetAllFlightsAsync_ReturnsFlights()
        {
            // Arrange
            var flights = new List<Flight> { new Flight { FlightNumber = "AB123" } };
            _mockFlightRepo.Setup(r => r.GetAll()).ReturnsAsync(flights);

            // Act
            var result = await _flightService.GetAllFlightsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("AB123", ((List<Flight>)result)[0].FlightNumber);
        }

        [Fact]
        public async Task GetFlightByIdAsync_ReturnsFlight_WhenFound()
        {
            var flight = new Flight { FlightId = "1", FlightNumber = "XY456" };
            _mockFlightRepo.Setup(r => r.GetById("1")).ReturnsAsync(flight);

            var result = await _flightService.GetFlightByIdAsync("1");

            Assert.NotNull(result);
            Assert.Equal("XY456", result!.FlightNumber);
        }

        [Fact]
        public async Task GetFlightByIdAsync_ReturnsNull_WhenNotFound()
        {
            _mockFlightRepo.Setup(r => r.GetById("1")).ReturnsAsync((Flight?)null);

            var result = await _flightService.GetFlightByIdAsync("1");

            Assert.Null(result);
        }

        [Fact]
        public async Task AddFlightAsync_CallsRepository()
        {
            var flight = new Flight { FlightNumber = "CD789" };

            await _flightService.AddFlightAsync(flight);

            _mockFlightRepo.Verify(r => r.AddAsync(flight), Times.Once);
        }

        [Fact]
        public async Task UpdateFlightAsync_CallsRepository()
        {
            var flight = new Flight { FlightNumber = "CD789" };

            await _flightService.UpdateFlightAsync(flight);

            _mockFlightRepo.Verify(r => r.UpdateAsync(flight), Times.Once);
        }

        [Fact]
        public async Task DeleteFlightAsync_CallsRepository()
        {
            await _flightService.DeleteFlightAsync("1");

            _mockFlightRepo.Verify(r => r.DeleteAsync("1"), Times.Once);
        }

        [Fact]
        public async Task SearchFlightsAsync_CallsRepositoryWithParams()
        {
            _mockFlightRepo.Setup(r => r.SearchFlightsAsync("USA", "UK", null, null, null, null, null))
                     .ReturnsAsync(new List<Flight> { new Flight { FlightNumber = "ZZ999" } });

            var result = await _flightService.SearchFlightsAsync(departureCountry: "USA", destinationCountry: "UK");

            Assert.Single(result);
            Assert.Equal("ZZ999", ((List<Flight>)result)[0].FlightNumber);
        }


        [Fact]
        public async Task IsFlightAvailableAsync_FlightExistsAndSeatsAvailable_ReturnsTrue()
        {
            // Arrange
            var flight = new Flight { FlightId = "F1", AvailableSeats = 5 };
            _mockFlightRepo.Setup(r => r.GetById("F1")).ReturnsAsync(flight);

            // Act
            var result = await _flightService.IsFlightAvailableAsync("F1", 3);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsFlightAvailableAsync_FlightNotExists_ReturnsFalse()
        {
            // Arrange
            _mockFlightRepo.Setup(r => r.GetById("F1")).ReturnsAsync((Flight?)null);

            // Act
            var result = await _flightService.IsFlightAvailableAsync("F1", 1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsFlightAvailableAsync_NotEnoughSeats_ReturnsFalse()
        {
            // Arrange
            var flight = new Flight { FlightId = "F1", AvailableSeats = 2 };
            _mockFlightRepo.Setup(r => r.GetById("F1")).ReturnsAsync(flight);

            // Act
            var result = await _flightService.IsFlightAvailableAsync("F1", 3);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ReserveSeatsAsync_ValidFlight_ReservesSeats()
        {
            // Arrange
            var flight = new Flight { FlightId = "F1", AvailableSeats = 10 };
            _mockFlightRepo.Setup(r => r.GetById("F1")).ReturnsAsync(flight);
            _mockFlightRepo.Setup(r => r.UpdateAsync(It.IsAny<Flight>())).Returns(Task.CompletedTask);

            // Act
            var result = await _flightService.ReserveSeatsAsync("F1", 3);

            // Assert
            Assert.True(result);
            Assert.Equal(7, flight.AvailableSeats);
        }

        [Fact]
        public async Task ReserveSeatsAsync_FlightNotFound_ThrowsException()
        {
            // Arrange
            _mockFlightRepo.Setup(r => r.GetById("F1")).ReturnsAsync((Flight?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _flightService.ReserveSeatsAsync("F1", 1));
        }
    }
}
