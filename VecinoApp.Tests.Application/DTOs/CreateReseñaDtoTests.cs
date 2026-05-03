using Xunit;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using VecinoApp.Application.DTOs;

namespace VecinoApp.Tests.Application.DTOs;

public class CreateReseñaDtoTests
{
    [Fact]
    public void CreateReseñaDto_ConDatosValidos_DeberiaPasarValidacion()
    {
        // Arrange
        var dto = new CreateReseñaDto
        {
            UsuarioId = 1,
            NegocioId = 1,
            Calificacion = 5,
            Comentario = "Excelente servicio!"
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void CreateReseñaDto_CalificacionInvalida_DeberiaFallar()
    {
        // Arrange
        var dto = new CreateReseñaDto
        {
            UsuarioId = 1,
            NegocioId = 1,
            Calificacion = 6,
            Comentario = "Comentario"
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

        // Assert
        isValid.Should().BeFalse();
    }
}