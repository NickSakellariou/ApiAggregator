using System;
using ApiAggregator.Utilities;
using Xunit;

namespace ApiAggregator.Tests.Utilities
{
    public class InputValidatorTests
    {
        [Fact]
        public void ValidateDateRange_ThrowsException_WhenStartDateAfterEndDate()
        {
            // Arrange
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => InputValidator.ValidateDateRange(startDate, endDate));
        }

        [Fact]
        public void ValidateDateRange_DoesNotThrow_WhenDatesAreValid()
        {
            // Arrange
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Act
            var exception = Record.Exception(() => InputValidator.ValidateDateRange(startDate, endDate));

            // Assert
            Assert.Null(exception);
        }
    }
}
