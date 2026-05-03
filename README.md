# 🏪 VecinoApp - Backend API

API RESTful en .NET 8 para la aplicación VeciNoApp, que permite gestionar negocios, reseñas, favoritos, productos y promociones.

## 📱 Repositorios

| Repositorio | Enlace |
|-------------|--------|
| **Frontend Flutter** | [VecinoApp](https://github.com/Frankkantor348/VecinoApp) |
| **Backend API** | [VecinoApp-API](https://github.com/Frankkantor348/VecinoApp-API) |
| **Swagger** | [Swagger-API](http://localhost:5067/swagger) |

## 🛠️ Tecnologías utilizadas

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| .NET | 8.0 | Framework principal |
| Entity Framework Core | 8.0 | ORM para base de datos |
| SQL Server | - | Base de datos relacional |
| ASP.NET Core Identity | 8.0 | Autenticación y usuarios |
| JWT | - | Tokens de autenticación |
| Swagger/OpenAPI | 6.5.0 | Documentación interactiva de API |

## 📋 Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (LocalDB o completo)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o superior
- [Git](https://git-scm.com/downloads)

## 🚀 Instalación y configuración

### 1. Clonar el repositorio

```bash
git clone https://github.com/Frankkantor348/VecinoApp-API.git
cd VecinoApp-API
2. Restaurar paquetes
bash

dotnet restore

3. Configurar la base de datos

Modifica la cadena de conexión en appsettings.json:
json

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VecinoAppDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}

4. Aplicar migraciones
bash

dotnet ef database update --project VecinoApp.Persistence --startup-project VecinoApp.API

5. Ejecutar la API
bash

cd VecinoApp.API
dotnet run

🧪 Probar la API con Swagger (Recomendado)

Una vez ejecutada la API, abre tu navegador en:
🔗 http://localhost:5067/swagger

Swagger te proporciona una interfaz interactiva donde puedes:

    ✅ Ver todos los endpoints disponibles

    ✅ Probar cada endpoint directamente desde el navegador

    ✅ Autenticarte con JWT para endpoints protegidos

    ✅ Ver los esquemas de datos (DTOs)

    ✅ Ejecutar peticiones y ver respuestas en tiempo real

🔐 Cómo autenticarte en Swagger

    Ve a AuthController → POST /api/Auth/login

    Haz clic en "Try it out"

    Ingresa las credenciales de prueba:

json

{
  "email": "test@vecinoapp.com",
  "password": "NuevaClave1234"
}

    Ejecuta y copia el token de la respuesta

    Haz clic en el botón "Authorize" (candado arriba a la derecha)

    Pega: Bearer {tu-token-copiado}

    Haz clic en "Authorize" y luego "Close"

¡Ya puedes probar todos los endpoints protegidos!
🔐 Autenticación

La API utiliza JWT (JSON Web Tokens) para autenticación.
Endpoints de autenticación
Método	Endpoint	Descripción
POST	/api/Auth/login	Iniciar sesión con email/contraseña
POST	/api/Auth/register	Registrar nuevo usuario
POST	/api/Auth/forgot-password	Solicitar recuperación de contraseña
POST	/api/Auth/reset-password	Restablecer contraseña
📁 Estructura del proyecto
text

VecinoApp/
├── VecinoApp.API/           # Controladores y punto de entrada
├── VecinoApp.Application/   # DTOs y lógica de aplicación
├── VecinoApp.Domain/        # Entidades e interfaces
├── VecinoApp.Persistence/   # Contexto de BD y repositorios
├── VecinoApp.Tests.API/     # Pruebas unitarias de controladores
├── VecinoApp.Tests.Application/ # Pruebas de DTOs
├── VecinoApp.Tests.Domain/  # Pruebas de entidades
└── VecinoApp.sln            # Solución de Visual Studio

📚 Endpoints principales
Negocios
Método	Endpoint	Descripción
GET	/api/Negocios/cercanos	Negocios cercanos por ubicación
GET	/api/Negocios/{id}	Detalle de negocio
POST	/api/Negocios	Registrar nuevo negocio
PUT	/api/Negocios/{id}	Actualizar negocio
GET	/api/Negocios/pendientes	Negocios por aprobar (Admin)
PUT	/api/Negocios/aprobar/{id}	Aprobar negocio (Admin)
Reseñas
Método	Endpoint	Descripción
GET	/api/Reseñas/negocio/{negocioId}	Reseñas de un negocio
POST	/api/Reseñas	Crear reseña
PUT	/api/Reseñas/{id}	Actualizar reseña
DELETE	/api/Reseñas/{id}	Eliminar reseña
Favoritos
Método	Endpoint	Descripción
GET	/api/Favoritos/negocios/usuario/{usuarioId}	Favoritos del usuario
POST	/api/Favoritos	Agregar a favoritos
DELETE	/api/Favoritos	Eliminar de favoritos
Productos
Método	Endpoint	Descripción
GET	/api/Productos/negocio/{negocioId}	Productos por negocio
POST	/api/Productos	Crear producto
PUT	/api/Productos/{id}	Actualizar producto
DELETE	/api/Productos/{id}	Eliminar producto
Promociones
Método	Endpoint	Descripción
GET	/api/Promociones/activas	Promociones activas
GET	/api/Promociones/negocio/{negocioId}	Promociones por negocio
POST	/api/Promociones	Crear promoción
PUT	/api/Promociones/{id}	Actualizar promoción
DELETE	/api/Promociones/{id}	Eliminar promoción
🧪 Pruebas unitarias

El backend incluye 151 pruebas unitarias que cubren:
Proyecto de pruebas	Cantidad	Qué prueba
VecinoApp.Tests.Domain	~20	Entidades (Negocio, Reseña, Usuario, etc.)
VecinoApp.Tests.Application	~10	DTOs y validaciones
VecinoApp.Tests.API	~121	Controladores y endpoints
Ejecutar las pruebas
bash

dotnet test

🔑 Credenciales de prueba
Email	Contraseña	Rol
test@vecinoapp.com	NuevaClave1234	Administrador
nuevoadmin@vecinoapp.com	Admin123!	Usuario (puede asignarse Admin)
📝 Ejemplos de peticiones con Swagger
1. Login (obtener token)
json

POST /api/Auth/login
{
  "email": "test@vecinoapp.com",
  "password": "NuevaClave1234"
}

2. Crear una reseña
json

POST /api/Reseñas
{
  "usuarioId": 3,
  "negocioId": 8,
  "calificacion": 5,
  "comentario": "Excelente servicio!"
}

3. Ver negocios cercanos
text

GET /api/Negocios/cercanos?latitud=4.7110&longitud=-74.0721&radioMetros=1000

4. Agregar a favoritos
json

POST /api/Favoritos
{
  "usuarioId": 3,
  "negocioId": 8
}

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

Swagger no carga

    Verifica que el programa tenga: app.UseSwagger() y app.UseSwaggerUI()

    Asegúrate de que AddEndpointsApiExplorer() esté configurado

📝 Nota

Google Sign-In no está disponible en esta versión. La autenticación se realiza mediante email y contraseña.
📞 Contacto

Desarrolladores: Frank Cantor, Dilan Jiménez
GitHub: Frankkantor348
📄 Licencia

Proyecto académico - Todos los derechos reservados
text




