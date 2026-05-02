# 🏪 VeciNoApp - Backend API

API RESTful en .NET 8 para la aplicación VeciNoApp, que permite gestionar negocios, reseñas, favoritos, productos y promociones.

## 📱 Repositorio Frontend

- **Frontend Flutter:** [VeciNoApp](https://github.com/Frankkantor348/VecinoApp)

## 🛠️ Tecnologías utilizadas

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| .NET | 8.0 | Framework principal |
| Entity Framework Core | 8.0 | ORM para base de datos |
| SQL Server | - | Base de datos relacional |
| ASP.NET Core Identity | 8.0 | Autenticación y usuarios |
| JWT | - | Tokens de autenticación |
| Swagger/OpenAPI | - | Documentación de API |

## 📋 Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (LocalDB o completo)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o superior

## 🚀 Instalación y configuración

### 1. Clonar el repositorio

```bash
git clone https://github.com/Frankkantor348/VecinoApp-API.git
cd VecinoApp-API

2. Configurar la base de datos

Modifica la cadena de conexión en appsettings.json:
json

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VecinoAppDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}

3. Aplicar migraciones
bash

dotnet ef database update --project VecinoApp.Persistence --startup-project VecinoApp.API

4. Ejecutar la API
bash

cd VecinoApp.API
dotnet run

La API estará disponible en:

    HTTP: http://localhost:5067

    HTTPS: https://localhost:7128

    Swagger: http://localhost:5067/swagger

🔐 Autenticación

La API utiliza JWT (JSON Web Tokens) para autenticación.
Endpoints de autenticación
Método	Endpoint	Descripción
POST	/api/auth/login	Iniciar sesión con email/contraseña
POST	/api/auth/register	Registrar nuevo usuario
POST	/api/auth/forgot-password	Solicitar recuperación de contraseña
POST	/api/auth/reset-password	Restablecer contraseña
📁 Estructura del proyecto
text

VecinoApp/
├── VecinoApp.API/           # Controladores y punto de entrada
├── VecinoApp.Application/   # DTOs y lógica de aplicación
├── VecinoApp.Domain/        # Entidades e interfaces
├── VecinoApp.Persistence/   # Contexto de BD y repositorios
└── VecinoApp.sln            # Solución de Visual Studio

📚 Endpoints principales
Negocios
Método	Endpoint	Descripción
GET	/api/negocios/cercanos	Negocios cercanos por ubicación
GET	/api/negocios/{id}	Detalle de negocio
POST	/api/negocios	Registrar nuevo negocio
PUT	/api/negocios/{id}	Actualizar negocio
GET	/api/negocios/pendientes	Negocios por aprobar (Admin)
PUT	/api/negocios/aprobar/{id}	Aprobar negocio (Admin)
Reseñas
Método	Endpoint	Descripción
GET	/api/reseñas/negocio/{negocioId}	Reseñas de un negocio
POST	/api/reseñas	Crear reseña
PUT	/api/reseñas/{id}	Actualizar reseña
DELETE	/api/reseñas/{id}	Eliminar reseña
Favoritos
Método	Endpoint	Descripción
GET	/api/favoritos/negocios/usuario/{usuarioId}	Favoritos del usuario
POST	/api/favoritos	Agregar a favoritos
DELETE	/api/favoritos	Eliminar de favoritos
Productos
Método	Endpoint	Descripción
GET	/api/productos/negocio/{negocioId}	Productos por negocio
POST	/api/productos	Crear producto
PUT	/api/productos/{id}	Actualizar producto
DELETE	/api/productos/{id}	Eliminar producto
Promociones
Método	Endpoint	Descripción
GET	/api/promociones/activas	Promociones activas
GET	/api/promociones/negocio/{negocioId}	Promociones por negocio
POST	/api/promociones	Crear promoción
PUT	/api/promociones/{id}	Actualizar promoción
DELETE	/api/promociones/{id}	Eliminar promoción
🔑 Credenciales de prueba
Email	Contraseña	Rol
test@vecinoapp.com	Abc123	Usuario normal
admin@vecinoapp.com	Admin123	Administrador
🧪 Probar la API con Swagger

    Ejecuta la API con dotnet run

    Ve a http://localhost:5067/swagger

    Prueba los endpoints directamente desde la interfaz

❓ Solución de problemas comunes
Error de conexión a base de datos

    Verifica la cadena de conexión en appsettings.json

    Asegúrate que SQL Server esté corriendo

Error de migraciones
bash

dotnet ef migrations add NombreMigracion --project VecinoApp.Persistence --startup-project VecinoApp.API
dotnet ef database update --project VecinoApp.Persistence --startup-project VecinoApp.API

El puerto 5067 ya está en uso
bash

dotnet run --urls http://localhost:5068

📝 Nota

Google Sign-In no está disponible en esta versión. La autenticación se realiza mediante email y contraseña.
📞 Contacto

Desarrollador: Frank Kantor
GitHub: Frankkantor348
📄 Licencia

Proyecto académico - Todos los derechos reservados
