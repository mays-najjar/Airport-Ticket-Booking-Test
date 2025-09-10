using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AirportBooking.Models;
using AirportBooking.Repositories;
using AirportBooking.Services;

namespace AirportBooking.Tests
{
    [Trait("Category", "BookingService")]
    public class BookingServiceTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepo;
        private readonly Mock<IFlightRepository> _mockFlightRepo;
        private readonly Mock<IPassengerRepository> _mockPassengerRepo;
        private readonly FlightService _flightService;
        private readonly PassengerService _passengerService;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _mockBookingRepo = new Mock<IBookingRepository>();
            _mockFlightRepo = new Mock<IFlightRepository>();
            _mockPassengerRepo = new Mock<IPassengerRepository>();

            _flightService = new FlightService(_mockFlightRepo.Object);
            _passengerService = new PassengerService(_mockPassengerRepo.Object);

            _bookingService = new BookingService(
                _mockBookingRepo.Object,
                _flightService,
                _passengerService
            );
        }

        [Fact]
        public async Task CreateBookingAsync_ValidData_ReturnsBooking()
        {
            // Arrange
            var passenger = new Passenger { PassengerId = "P1", Email = "test@mail.com" };
            var flight = new Flight { FlightId = "F1", Price = 100, AvailableSeats = 10 };

            _mockPassengerRepo.Setup(r => r.GetByEmailAsync("test@mail.com"))
                .ReturnsAsync(passenger);

            _mockFlightRepo.Setup(r => r.GetById("F1"))
                .ReturnsAsync(flight);

            _mockBookingRepo.Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bookingService.CreateBookingAsync("test@mail.com", "F1", "Economy", 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("F1", result.FlightId);
            Assert.Equal("P1", result.PassengerId);
            Assert.Equal(2, result.NumberOfSeats);
            Assert.True(result.TotalPrice > 0);
        }

        [Fact]
        public async Task GetAllBookingsAsync_ReturnsAllBookings()
        {
            // Arrange
            var bookings = new List<Booking>
            {
                new Booking { BookingId = "B1", FlightId = "F1", PassengerId = "P1" },
                new Booking { BookingId = "B2", FlightId = "F2", PassengerId = "P2" }
            };
            _mockBookingRepo.Setup(r => r.GetAll()).ReturnsAsync(bookings);

            // Act
            var result = await _bookingService.GetAllBookingsAsync();

            // Assert
            Assert.Equal(2, ((List<Booking>)result).Count);
        }

        [Fact]
        public async Task GetBookingByIdAsync_ReturnsBooking_WhenExists()
        {
            // Arrange
            var booking = new Booking { BookingId = "B1", FlightId = "F1", PassengerId = "P1" };
            _mockBookingRepo.Setup(r => r.GetById("B1")).ReturnsAsync(booking);

            // Act
            var result = await _bookingService.GetBookingByIdAsync("B1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("B1", result!.BookingId);
        }

        [Fact]
        public async Task CancelBookingAsync_ReturnsTrue_WhenValidBooking()
        {
            // Arrange
            var booking = new Booking { BookingId = "B1", FlightId = "F1", PassengerId = "P1", NumberOfSeats = 2 };
            _mockBookingRepo.Setup(r => r.GetById("B1")).ReturnsAsync(booking);
            _mockBookingRepo.Setup(r => r.UpdateAsync(booking)).Returns(Task.CompletedTask);
            _mockFlightRepo.Setup(r => r.GetById("F1")).ReturnsAsync(new Flight { FlightId = "F1", AvailableSeats = 5 });
            _mockFlightRepo.Setup(r => r.UpdateAsync(It.IsAny<Flight>())).Returns(Task.CompletedTask);

            // Act
            var result = await _bookingService.CancelBookingAsync("B1");

            // Assert
            Assert.True(result);
            Assert.True(booking.IsCancelled);
        }

        [Fact]
        public async Task ManageBookingsAsync_ReturnsPassengerBookings()
        {
            // Arrange
            var passenger = new Passenger { PassengerId = "P1", Email = "ali@test.com" };
            var bookings = new List<Booking>
            {
                new Booking { BookingId = "B1", FlightId = "F1", PassengerId = "P1" }
            };

            _mockPassengerRepo.Setup(r => r.GetByEmailAsync("ali@test.com")).ReturnsAsync(passenger);
            _mockBookingRepo.Setup(r => r.GetAll()).ReturnsAsync(bookings);

            // Act
            var result = await _bookingService.ManageBookingsAsync("ali@test.com");

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task ModifyBookingAsync_UpdatesBooking_WhenValid()
        {
            // Arrange
            var booking = new Booking
            {
                BookingId = "B1",
                FlightId = "F1",
                PassengerId = "P1",
                NumberOfSeats = 2,
                SelectedClass = FlightClass.Economy,
                TotalPrice = 200
            };

            var flight = new Flight { FlightId = "F1", Price = 100, AvailableSeats = 10 };

            _mockBookingRepo.Setup(r => r.GetById("B1")).ReturnsAsync(booking);
            _mockFlightRepo.Setup(r => r.GetById("F1")).ReturnsAsync(flight);
            _mockBookingRepo.Setup(r => r.UpdateAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);
            _mockFlightRepo.Setup(r => r.UpdateAsync(It.IsAny<Flight>())).Returns(Task.CompletedTask);

            // Act
            var result = await _bookingService.ModifyBookingAsync("B1", FlightClass.Business, 3);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, booking.NumberOfSeats);
            Assert.Equal(FlightClass.Business, booking.SelectedClass);
        }
    }
}
