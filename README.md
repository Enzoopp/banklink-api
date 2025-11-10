# 🏦 BankLink API

Sistema bancario completo desarrollado en **ASP.NET Core 9.0** que permite gestionar clientes, cuentas y transferencias interbancarias en tiempo real.

## 🚀 Características Principales

- ✅ **Gestión de Clientes y Cuentas**: CRUD completo de clientes y cuentas bancarias
- ✅ **Operaciones Bancarias**: Depósitos, retiros y consulta de movimientos
- ✅ **Transferencias Interbancarias**: Envío y recepción de transferencias con bancos externos
- ✅ **Idempotencia**: Prevención de operaciones duplicadas
- ✅ **Resiliencia**: Implementación de Polly (Retry, Circuit Breaker, Timeout)
- ✅ **Transacciones Atómicas**: Garantía de consistencia en operaciones críticas
- ✅ **Autenticación JWT**: Sistema de autenticación seguro con roles
- ✅ **Validación Multi-capa**: FluentValidation + Data Annotations

## 🛠️ Tecnologías Utilizadas

- **Framework**: ASP.NET Core 9.0
- **ORM**: Entity Framework Core 9
- **Base de Datos**: SQL Server
- **Autenticación**: ASP.NET Core Identity + JWT
- **Validación**: FluentValidation
- **Resiliencia**: Polly
- **Documentación**: Swagger/OpenAPI

## 📋 Requisitos Previos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (LocalDB, Express o superior)
- Visual Studio 2022, VS Code o Rider (opcional)

## 🔧 Instalación y Configuración

### 1. Clonar el repositorio
```bash
git clone https://github.com/TU_USUARIO/banklink.git
cd banklink/BankLink.Api
```

### 2. Configurar la cadena de conexión
Edita `appsettings.json` y configura tu conexión a SQL Server:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BankLinkDb;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

### 3. Restaurar paquetes NuGet
```bash
dotnet restore
```

### 4. Aplicar migraciones
```bash
dotnet ef database update
```

### 5. Ejecutar la aplicación
```bash
dotnet run
```

La API estará disponible en:
- **HTTPS**: `https://localhost:7xxx`
- **HTTP**: `http://localhost:5xxx`
- **Swagger**: `https://localhost:7xxx/swagger`

## 👤 Usuario Inicial

Al iniciar la aplicación, se crea automáticamente un usuario administrador:

- **Email**: `admin@banklink.local`
- **Contraseña**: `Admin123!`

## 📚 Endpoints Principales

### Autenticación
- `POST /api/Auth/register` - Registrar nuevo usuario
- `POST /api/Auth/login` - Iniciar sesión
- `GET /api/Auth/me` - Obtener información del usuario actual

### Clientes
- `GET /api/Clientes` - Listar todos los clientes
- `GET /api/Clientes/{id}` - Obtener cliente por ID
- `POST /api/Clientes` - Crear nuevo cliente
- `PUT /api/Clientes/{id}` - Actualizar cliente
- `DELETE /api/Clientes/{id}` - Eliminar cliente

### Cuentas
- `GET /api/Cuentas` - Listar todas las cuentas
- `GET /api/Cuentas/{id}` - Obtener cuenta por ID
- `POST /api/Cuentas` - Crear nueva cuenta
- `PUT /api/Cuentas/{id}/estado` - Activar/Inactivar cuenta
- `POST /api/Cuentas/{id}/depositos` - Realizar depósito
- `POST /api/Cuentas/{id}/retiros` - Realizar retiro
- `GET /api/Cuentas/{id}/movimientos` - Ver historial de movimientos

### Transferencias Interbancarias
- `POST /api/Transferencias/enviar` - Enviar transferencia a banco externo
- `POST /api/Transferencias/recibir` - Recibir transferencia desde banco externo

### Bancos Externos
- `GET /api/BancosExternos` - Listar bancos registrados
- `GET /api/BancosExternos/{id}` - Obtener banco por ID
- `POST /api/BancosExternos` - Registrar nuevo banco externo
- `PUT /api/BancosExternos/{id}` - Actualizar banco
- `DELETE /api/BancosExternos/{id}` - Eliminar banco

## 🔐 Autenticación

La API usa **JWT (JSON Web Tokens)** para autenticación. Para usar endpoints protegidos:

1. Obtén un token mediante `POST /api/Auth/login`
2. Incluye el token en el header: `Authorization: Bearer {tu_token}`

## 💡 Ejemplos de Uso

### Crear un cliente
```http
POST /api/Clientes
Authorization: Bearer {token}
Content-Type: application/json

{
  "nombre": "Juan",
  "apellido": "Pérez",
  "dni": "12345678",
  "email": "juan@example.com",
  "telefono": "1234567890",
  "direccion": "Calle Falsa 123"
}
```

### Crear una cuenta con depósito inicial
```http
POST /api/Cuentas
Authorization: Bearer {token}
Content-Type: application/json

{
  "accountNumber": "0001234567890",
  "type": 1,
  "clientId": 1,
  "initialBalance": 10000
}
```

### Transferencia interbancaria
```http
POST /api/Transferencias/enviar
Authorization: Bearer {token}
Content-Type: application/json

{
  "originAccountId": 1,
  "destinationBankCode": "BANCO_NACION",
  "destinationAccountNumber": "0110599520000001234567",
  "amount": 1500.50,
  "concept": "Pago de factura",
  "idempotencyKey": "TRX-2025-11-10-001"
}
```

## 🏗️ Arquitectura del Proyecto

```
BankLink.Api/
├── Controllers/          # Endpoints REST
├── Services/            # Lógica de negocio
├── Data/                # DbContext y configuración EF
├── Domain/              # Entidades del modelo
├── Dtos/                # Data Transfer Objects
├── Validators/          # FluentValidation
├── Constants/           # Valores constantes
└── Migrations/          # Migraciones de EF Core
```

## 🔄 Características Técnicas Destacadas

### Idempotencia
Las transferencias implementan idempotencia mediante `IdempotencyKey`, garantizando que una operación solo se ejecute una vez incluso si se reintenta múltiples veces.

### Resiliencia con Polly
Políticas implementadas para comunicación con bancos externos:
- **Retry Policy**: 3 reintentos con backoff exponencial (2s, 4s, 8s)
- **Circuit Breaker**: Se abre tras 5 fallos consecutivos
- **Timeout**: 30 segundos máximo por petición

### Transacciones Atómicas
Todas las operaciones críticas usan transacciones de base de datos que garantizan:
- **Atomicidad**: Todo o nada
- **Consistencia**: Estado siempre válido
- **Rollback automático** en caso de fallo

## 📊 Modelo de Datos

### Entidades Principales
- **Client**: Clientes bancarios
- **Account**: Cuentas (Ahorro/Corriente)
- **Movement**: Movimientos (Depósitos/Retiros/Transferencias)
- **ExternalBank**: Bancos externos registrados
- **AppUser**: Usuarios del sistema (Identity)

## 🧪 Testing

Para ejecutar tests (cuando estén implementados):
```bash
dotnet test
```

## 📝 Mejoras Futuras

- [ ] Tests unitarios e integración
- [ ] Encriptación de API Keys en base de datos
- [ ] Paginación en listados
- [ ] Soft deletes
- [ ] Sistema de notificaciones
- [ ] Rate limiting
- [ ] Cache con Redis
- [ ] Dockerización
- [ ] CI/CD con GitHub Actions

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Por favor:
1. Fork el proyecto
2. Crea una rama (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto es de código abierto y está disponible bajo la licencia MIT.

## 👨‍💻 Autor

**[Tu Nombre]**
- GitHub: [@TU_USUARIO](https://github.com/TU_USUARIO)
- Email: tu_email@ejemplo.com

## 🙏 Agradecimientos

- ASP.NET Core Team
- Entity Framework Core Team
- Polly Contributors
- FluentValidation Contributors

---

⭐ Si este proyecto te fue útil, no olvides darle una estrella!
