using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AirportBooking.Models;
using Xunit;

namespace AirportBookingTest
{
    [Trait("Category", "Flight")]
    public class FlightTests
    {
        [Theory]
        [InlineData(FlightClass.Economy, 100, 100)]
        [InlineData(FlightClass.Business, 100, 250)]
        [InlineData(FlightClass.FirstClass, 100, 400)]
        public void GetPriceForClass_ReturnsCorrectPrice(FlightClass flightClass, decimal basePrice, decimal expected)
        {
            // Arrange
            var flight = new Flight { Price = basePrice };

            // Act
            var result = flight.GetPriceForClass(flightClass);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToString_ReturnsExpectedFormat()
        {
            // Arrange
            var date = new DateTime(2025, 1, 1, 14, 0, 0);
            var flight = new Flight
            {
                FlightNumber = "XY123",
                DepartureCountry = "Palestine",
                DestinationCountry = "Jordan",
                DepartureDate = date
            };

            // Act
            var result = flight.ToString();

            // Assert
            Assert.Equal("XY123 - Palestine to Jordan - 2025-01-01 14:00", result);
        }

        [Fact]
        public void ValidateFlight_AvailableSeatsNegative_ReturnsError()
        {
            // Arrange
            var flight = new Flight
            {
                FlightNumber = "CD456",
                DepartureCountry = "Spain",
                DestinationCountry = "Italy",
                DepartureDate = DateTime.Now,
                DepartureAirport = "Madrid",
                ArrivalAirport = "Rome",
                Price = 120,
                AvailableSeats = -5
            };

            var context = new ValidationContext(flight);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(flight, context, results, true);

            // Assert
            Assert.False(isValid);
            var errorMessages = results.Select(r => r.ErrorMessage ?? string.Empty).ToList();
            Assert.Contains("Available seats must be non-negative", errorMessages);
        }

        [Fact]
        public void ValidateFlight_MissingRequiredFields_ReturnsErrors()
        {
            // Arrange
            var flight = new Flight();

            var context = new ValidationContext(flight);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(flight, context, results, true);

            // Assert
            Assert.False(isValid);
            var errorMessages = results.Select(r => r.ErrorMessage ?? string.Empty).ToList();

            Assert.Contains("Flight number is required", errorMessages);
            Assert.Contains("Departure country is required", errorMessages);
            Assert.Contains("Arrival country is required", errorMessages);
        }

    }

}
