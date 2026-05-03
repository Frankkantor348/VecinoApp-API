using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using VecinoApp.Domain.Entities;
using VecinoApp.Domain.Interfaces;
using VecinoApp.Infrastructure.Services;
using VecinoApp.Persistence.Data;
using VecinoApp.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONFIGURAR URLS para escuchar en todas las interfaces
// ============================================================
builder.WebHost.UseUrls("http://0.0.0.0:5067");

// ============================================================
// CONFIGURAR WebRootPath (wwwroot) para guardar imágenes
// ============================================================
if (string.IsNullOrEmpty(builder.Environment.WebRootPath))
{
    builder.Environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
}

// Crear la carpeta wwwroot si no existe
if (!Directory.Exists(builder.Environment.WebRootPath))
{
    Directory.CreateDirectory(builder.Environment.WebRootPath);
}

// Crear subcarpetas necesarias
var uploadsPath = Path.Combine(builder.Environment.WebRootPath, "uploads", "negocios");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// ============================================================
// Add services to the container.
// ============================================================
builder.Services.AddControllers();

// ============================================================
// CONFIGURAR SWAGGER (NUEVO)
// ============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VecinoApp API",
        Version = "v1",
        Description = "API para la aplicación VecinoApp - Descubre negocios cercanos",
        Contact = new OpenApiContact
        {
            Name = "Frank Kantor",
            Email = "fgutie16@estudiante.ibero.edu.co", 
            Url = new Uri("https://github.com/Frankkantor348")
        }
    });

    // Configurar autenticación JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT con el formato: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
// ============================================================
// FIN SWAGGER
// ============================================================

// Configurar DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Geolocalización: soporte para datos espaciales
        sqlOptions.UseNetTopologySuite();
    }));

// Configurar Identity
builder.Services.AddIdentity<Usuario, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// REGISTRO DE REPOSITORIOS (inyección de dependencias)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<INegocioRepository, NegocioRepository>();
builder.Services.AddScoped<IReseñaRepository, ReseñaRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IPromocionRepository, PromocionRepository>();
builder.Services.AddScoped<IFavoritoRepository, FavoritoRepository>();
builder.Services.AddScoped<IFileService, FileService>();

// CONFIGURACIÓN CORS (para permitir peticiones desde Flutter)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutter", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5067",           // Pruebas locales
                "http://10.0.2.2:5067",            // Emulador Android
                "http://192.168.20.9:5067"         // ip para pruebas desde dispositivos físicos
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Configurar JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not found."));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ============================================================
// CREAR ROLES POR DEFECTO AL INICIAR LA APLICACIÓN
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

    string[] roles = { "Usuario", "Admin", "Propietario" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
            Console.WriteLine($"✅ Rol '{role}' creado");
        }
    }
}

// ============================================================
// PIPELINE DE LA APLICACIÓN (el orden es importante)
// ============================================================

// ============================================================
// AGREGAR SWAGGER MIDDLEWARE (NUEVO)
// ============================================================
// Habilitar Swagger (solo en desarrollo, pero puedes cambiarlo)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VeciNoApp API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "VeciNoApp API Documentation";
    });
}
// ============================================================
// FIN SWAGGER MIDDLEWARE
// ============================================================

// Servir archivos estáticos (IMPORTANTE para las imágenes)
app.UseStaticFiles();

// CORS debe ir ANTES de Authentication/Authorization
app.UseCors("AllowFlutter");

app.UseHttpsRedirection();
app.UseAuthentication();            // Autenticación
app.UseAuthorization();             // Autorización
app.MapControllers();

// Log de inicio
Console.WriteLine($"✅ WebRootPath configurado en: {builder.Environment.WebRootPath}");
Console.WriteLine($"✅ Carpeta de uploads: {uploadsPath}");
Console.WriteLine("✅ API iniciada correctamente");

app.Run();