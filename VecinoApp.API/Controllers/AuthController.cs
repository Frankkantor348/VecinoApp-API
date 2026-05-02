using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VecinoApp.Application.DTOs;
using VecinoApp.Domain.Entities;

namespace VecinoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return BadRequest("El email ya está registrado");

            var user = new Usuario
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                Nombre = registerDto.Nombre,
                FechaRegistro = DateTime.UtcNow,
                Activo = true
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Usuario");

            var token = await GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Email = user.Email,
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(60)
            });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Unauthorized("Email o contraseña incorrectos");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
                return Unauthorized("Email o contraseña incorrectos");

            if (!user.Activo)
                return Unauthorized("Usuario inactivo");

            var token = await GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Email = user.Email,
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(60)
            });
        }

        // POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            Console.WriteLine("=== SOLICITUD DE RECUPERACIÓN ===");
            Console.WriteLine($"Email solicitado: {forgotPasswordDto.Email}");

            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

            if (user == null)
            {
                Console.WriteLine($"❌ Email no encontrado: {forgotPasswordDto.Email}");
                return Ok(new { message = "Si el email está registrado, recibirás un enlace para restablecer tu contraseña." });
            }

            Console.WriteLine($"✅ Usuario encontrado: {user.Email}");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            Console.WriteLine($"📝 Token generado: {token}");

            var encodedToken = Uri.EscapeDataString(token);
            var resetLink = $"tuapp://reset-password?email={user.Email}&token={encodedToken}";

            Console.WriteLine("=========================================");
            Console.WriteLine("🔐 ENLACE PARA RESTABLECER CONTRASEÑA:");
            Console.WriteLine(resetLink);
            Console.WriteLine("=========================================");
            Console.WriteLine($"📝 Token codificado: {encodedToken}");
            Console.WriteLine("=========================================");

            return Ok(new
            {
                message = "Si el email está registrado, recibirás un enlace para restablecer tu contraseña.",
                resetLink = resetLink,
                token = encodedToken,
                email = user.Email
            });
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            Console.WriteLine("=== RESTABLECER CONTRASEÑA ===");
            Console.WriteLine($"Email: {resetPasswordDto.Email}");
            Console.WriteLine($"Token recibido: {resetPasswordDto.Token}");
            Console.WriteLine($"Nueva contraseña: {resetPasswordDto.NewPassword}");

            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            {
                Console.WriteLine("❌ Las contraseñas no coinciden");
                return BadRequest(new { message = "Las contraseñas no coinciden" });
            }

            if (resetPasswordDto.NewPassword.Length < 6)
            {
                Console.WriteLine("❌ La contraseña es demasiado corta");
                return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });
            }

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                Console.WriteLine($"❌ Usuario no encontrado: {resetPasswordDto.Email}");
                return BadRequest(new { message = "Email no encontrado" });
            }

            Console.WriteLine($"✅ Usuario encontrado: {user.Email}");

            var decodedToken = Uri.UnescapeDataString(resetPasswordDto.Token);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, resetPasswordDto.NewPassword);

            if (result.Succeeded)
            {
                Console.WriteLine("✅ Contraseña restablecida correctamente");
                return Ok(new { message = "Contraseña restablecida correctamente" });
            }

            Console.WriteLine("❌ Error al restablecer contraseña:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"   - {error.Code}: {error.Description}");
            }

            return BadRequest(new { message = "Error al restablecer la contraseña. El enlace puede haber expirado o ser inválido." });
        }

        // ============================================================
        // GOOGLE LOGIN
        // ============================================================

        // POST: api/auth/google-login
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto request)
        {
            try
            {
                Console.WriteLine("=== GOOGLE LOGIN ===");
                Console.WriteLine($"ServerAuthCode recibido: {request.ServerAuthCode?.Substring(0, Math.Min(50, request.ServerAuthCode?.Length ?? 0))}...");

                // Validar el código con Google (usa TU WEB CLIENT ID)
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.ServerAuthCode,
                    new GoogleJsonWebSignature.ValidationSettings()
                    {
                        Audience = new[] { "523415817476-ra15u5sc3bco0au2s0gc1sk22m2hs4p5.apps.googleusercontent.com" }
                    });

                var email = payload.Email;
                Console.WriteLine($"✅ Email validado: {email}");
                Console.WriteLine($"✅ Nombre: {payload.Name}");

                // Buscar o crear usuario
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    Console.WriteLine("📝 Usuario no existe, creando nuevo...");
                    user = new Usuario
                    {
                        UserName = email,
                        Email = email,
                        Nombre = payload.Name ?? email.Split('@')[0],
                        FechaRegistro = DateTime.UtcNow,
                        Activo = true
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        Console.WriteLine("❌ Error al crear usuario");
                        return BadRequest(new { message = "Error al crear usuario", errors = createResult.Errors });
                    }

                    await _userManager.AddToRoleAsync(user, "Usuario");
                    Console.WriteLine("✅ Usuario creado exitosamente");
                }
                else
                {
                    Console.WriteLine("✅ Usuario ya existe");
                }

                // Generar token JWT de VecinoApp
                var token = await GenerateJwtToken(user);

                Console.WriteLine("✅ Login con Google exitoso");
                Console.WriteLine($"📦 Usuario ID: {user.Id}");
                Console.WriteLine($"📧 Email: {user.Email}");
                Console.WriteLine($"🔑 Token generado");

                return Ok(new AuthResponseDto
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Email = user.Email,
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(60)
                });
            }
            catch (InvalidJwtException ex)
            {
                Console.WriteLine($"❌ Token de Google inválido: {ex.Message}");
                return BadRequest(new { message = "El código de Google es inválido o expiró" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inesperado: {ex.Message}");
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        private async Task<string> GenerateJwtToken(Usuario user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException());

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Nombre", user.Nombre),
                new Claim("UserId", user.Id.ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(secretKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}