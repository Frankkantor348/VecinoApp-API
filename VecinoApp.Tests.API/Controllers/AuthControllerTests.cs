using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using VecinoApp.API.Controllers;
using VecinoApp.Application.DTOs;
using VecinoApp.Domain.Entities;
using Google.Apis.Auth;

namespace VecinoApp.Tests.API.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<Usuario>> _mockUserManager;
    private readonly Mock<SignInManager<Usuario>> _mockSignInManager;
    private readonly IConfiguration _configuration;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockUserManager = CreateMockUserManager();
        _mockSignInManager = CreateMockSignInManager();
        _configuration = CreateMockConfiguration();
        _controller = new AuthController(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _configuration);
    }

    private static Mock<UserManager<Usuario>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<Usuario>>();
        return new Mock<UserManager<Usuario>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static Mock<SignInManager<Usuario>> CreateMockSignInManager()
    {
        var context = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
        var userManager = CreateMockUserManager();
        return new Mock<SignInManager<Usuario>>(
            userManager.Object,
            new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object,
            new Mock<Microsoft.AspNetCore.Identity.IUserClaimsPrincipalFactory<Usuario>>().Object,
            null!, null!, null!, null!);
    }

    private static IConfiguration CreateMockConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"JwtSettings:SecretKey", "VecinoApp_Test_Secret_Key_For_Unit_Tests_1234567890"},
            {"JwtSettings:Issuer", "VecinoAppTest"},
            {"JwtSettings:Audience", "VecinoAppTestUsers"},
            {"JwtSettings:ExpirationMinutes", "60"}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    // ============================================================
    // Pruebas para Register
    // ============================================================

    [Fact]
    public async Task Register_ConDatosValidos_DeberiaCrearUsuarioYRetornarOk()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Nombre = "Test User",
            Email = "test@example.com",
            Password = "Password123!"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync((Usuario?)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<Usuario>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<Usuario>(), "Usuario"))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<Usuario>()))
            .ReturnsAsync(new List<string> { "Usuario" });

        // Act
        var resultado = await _controller.Register(registerDto);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Register_EmailYaRegistrado_DeberiaRetornarBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Nombre = "Test User",
            Email = "existing@example.com",
            Password = "Password123!"
        };

        var existingUser = new Usuario { Email = registerDto.Email };
        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync(existingUser);

        // Act
        var resultado = await _controller.Register(registerDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_ModelStateInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Email", "El email es requerido");
        var registerDto = new RegisterDto();

        // Act
        var resultado = await _controller.Register(registerDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_FalloEnCreacion_DeberiaRetornarBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Nombre = "Test User",
            Email = "test@example.com",
            Password = "Password123!"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync((Usuario?)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<Usuario>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error al crear usuario" }));

        // Act
        var resultado = await _controller.Register(registerDto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ============================================================
    // Pruebas para Login
    // ============================================================

    [Fact]
    public async Task Login_ConCredencialesValidas_DeberiaRetornarOkConToken()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var user = new Usuario
        {
            Id = 1,
            Email = loginDto.Email,
            Nombre = "Test User",
            Activo = true
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Usuario" });

        // Act
        var resultado = await _controller.Login(loginDto);

        // Assert
        resultado.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Login_UsuarioNoExistente_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _controller.Login(loginDto);

        // Assert
        resultado.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_ContraseñaIncorrecta_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var user = new Usuario { Email = loginDto.Email, Activo = true };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        var resultado = await _controller.Login(loginDto);

        // Assert
        resultado.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_UsuarioInactivo_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var user = new Usuario { Email = loginDto.Email, Activo = false };

        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        // Act
        var resultado = await _controller.Login(loginDto);

        // Assert
        resultado.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // ============================================================
    // Pruebas para ForgotPassword
    // ============================================================

    [Fact]
    public async Task ForgotPassword_EmailExistente_DeberiaRetornarOkConEnlace()
    {
        // Arrange
        var forgotPasswordDto = new ForgotPasswordDto { Email = "test@example.com" };
        var user = new Usuario { Email = forgotPasswordDto.Email };

        _mockUserManager.Setup(x => x.FindByEmailAsync(forgotPasswordDto.Email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("test-token");

        // Act
        var resultado = await _controller.ForgotPassword(forgotPasswordDto);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ForgotPassword_EmailNoExistente_DeberiaRetornarOkConMensaje()
    {
        // Arrange
        var forgotPasswordDto = new ForgotPasswordDto { Email = "nonexistent@example.com" };
        _mockUserManager.Setup(x => x.FindByEmailAsync(forgotPasswordDto.Email))
            .ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _controller.ForgotPassword(forgotPasswordDto);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // Pruebas para ResetPassword
    // ============================================================

    [Fact]
    public async Task ResetPassword_ConDatosValidos_DeberiaRestablecerContraseña()
    {
        // Arrange
        var resetPasswordDto = new ResetPasswordDto
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        var user = new Usuario { Email = resetPasswordDto.Email };

        _mockUserManager.Setup(x => x.FindByEmailAsync(resetPasswordDto.Email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var resultado = await _controller.ResetPassword(resetPasswordDto);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_ContraseñasNoCoinciden_DeberiaRetornarBadRequest()
    {
        // Arrange
        var resetPasswordDto = new ResetPasswordDto
        {
            Email = "test@example.com",
            Token = "token",
            NewPassword = "Password123!",
            ConfirmPassword = "DifferentPassword"
        };

        // Act
        var resultado = await _controller.ResetPassword(resetPasswordDto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_ContraseñaMuyCorta_DeberiaRetornarBadRequest()
    {
        // Arrange
        var resetPasswordDto = new ResetPasswordDto
        {
            Email = "test@example.com",
            Token = "token",
            NewPassword = "123",
            ConfirmPassword = "123"
        };

        // Act
        var resultado = await _controller.ResetPassword(resetPasswordDto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_UsuarioNoExistente_DeberiaRetornarBadRequest()
    {
        // Arrange
        var resetPasswordDto = new ResetPasswordDto
        {
            Email = "nonexistent@example.com",
            Token = "token",
            NewPassword = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(resetPasswordDto.Email))
            .ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _controller.ResetPassword(resetPasswordDto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }
}