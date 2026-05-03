using Xunit;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using VecinoApp.Application.DTOs;

namespace VecinoApp.Tests.Application.DTOs;

public class LoginDtoTests
{
    [Fact]
    public void LoginDto_ConDatosValidos_DeberiaPasar()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void LoginDto_SinEmail_DeberiaFallar()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "",
            Password = "Password123!"
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

        // Assert
        isValid.Should().BeFalse();
    }
}