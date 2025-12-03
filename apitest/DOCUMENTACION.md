# LocaScan - Documentación del Proyecto

## Descripción General

**LocaScan** es una aplicación móvil desarrollada en **.NET MAUI** para la gestión de inventario mediante escaneo de códigos de barras y geolocalización. La aplicación permite a los usuarios escanear productos, asociarlos a ubicaciones/almacenes específicos y sincronizar los datos con Firebase en tiempo real.

---

## Tabla de Contenidos

1. [Tecnologías Utilizadas](#tecnologías-utilizadas)
2. [Arquitectura del Proyecto](#arquitectura-del-proyecto)
3. [Estructura de Archivos](#estructura-de-archivos)
4. [Modelos de Datos](#modelos-de-datos)
5. [Servicios](#servicios)
6. [Páginas de la Aplicación](#páginas-de-la-aplicación)
7. [Base de Datos Firebase](#base-de-datos-firebase)
8. [Configuración y Preferencias](#configuración-y-preferencias)
9. [Funcionalidades Principales](#funcionalidades-principales)
10. [Permisos Requeridos](#permisos-requeridos)
11. [Instalación y Configuración](#instalación-y-configuración)
12. [Información del Desarrollador](#información-del-desarrollador)

---

## Tecnologías Utilizadas

| Tecnología | Versión | Descripción |
|------------|---------|-------------|
| .NET MAUI | 9.0 | Framework multiplataforma para apps móviles |
| C# | 12.0 | Lenguaje de programación principal |
| Firebase Authentication | - | Autenticación de usuarios |
| Firebase Realtime Database | - | Base de datos en tiempo real |
| ZXing.Net.MAUI | - | Escaneo de códigos de barras y QR |
| Plugin.Firebase | - | Integración con servicios de Firebase |

### Paquetes NuGet

```xml
<PackageReference Include="Plugin.Firebase.Auth" />
<PackageReference Include="Plugin.Firebase.Crashlytics" />
<PackageReference Include="FirebaseDatabase.net" />
<PackageReference Include="ZXing.Net.Maui.Controls" />
```

---

## Arquitectura del Proyecto

La aplicación sigue una arquitectura **MVVM simplificada** con separación de responsabilidades:

```
┌─────────────────────────────────────────────────────────┐
│                      PRESENTACIÓN                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │  MainPage   │  │InventoryPage│  │LocationsPage│     │
│  │  (Login)    │  │   (CRUD)    │  │   (CRUD)    │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │ SplashPage  │  │SettingsPage │  │  HomePage   │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│                       SERVICIOS                          │
│  ┌─────────────────────┐  ┌─────────────────────┐       │
│  │FirebaseDatabaseService│  │  PreferencesService │       │
│  └─────────────────────┘  └─────────────────────┘       │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│                       MODELOS                            │
│  ┌─────────────┐  ┌─────────────────┐                   │
│  │ ProductScan │  │ StorageLocation │                   │
│  └─────────────┘  └─────────────────┘                   │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│                   ALMACENAMIENTO                         │
│  ┌─────────────────────┐  ┌─────────────────────┐       │
│  │ Firebase Realtime DB │  │    Preferences      │       │
│  │     (Remoto)         │  │     (Local)         │       │
│  └─────────────────────┘  └─────────────────────┘       │
└─────────────────────────────────────────────────────────┘
```

---

## Estructura de Archivos

```
apitest/
├── App.xaml                    # Recursos globales de la aplicación
├── App.xaml.cs                 # Clase principal de la aplicación
├── AppShell.xaml               # Shell de navegación
├── AppShell.xaml.cs
├── MauiProgram.cs              # Configuración de servicios
│
├── Models/
│   ├── ProductScan.cs          # Modelo de producto escaneado
│   └── Location.cs             # Modelo de ubicación/almacén (StorageLocation)
│
├── Services/
│   ├── FirebaseDatabaseService.cs  # Servicio CRUD para Firebase
│   └── PreferencesService.cs       # Servicio de preferencias locales
│
├── Pages/
│   ├── MainPage.xaml(.cs)      # Página de login
│   ├── SplashPage.xaml(.cs)    # Pantalla de carga inicial
│   ├── HomePage.xaml(.cs)      # Página principal (legacy)
│   ├── InventoryPage.xaml(.cs) # Gestión de inventario
│   ├── LocationsPage.xaml(.cs) # Gestión de ubicaciones
│   └── SettingsPage.xaml(.cs)  # Configuración de la app
│
├── Resources/
│   ├── Images/
│   │   └── logo_locascan.png   # Logo de la aplicación
│   └── Styles/
│       ├── Colors.xaml         # Paleta de colores Material Design
│       └── Styles.xaml         # Estilos globales
│
└── Platforms/
    └── Android/
        ├── AndroidManifest.xml # Permisos de Android
        └── google-services.json # Configuración de Firebase
```

---

## Modelos de Datos

### ProductScan

Representa un producto escaneado en el inventario.

```csharp
public class ProductScan
{
    public string? Id { get; set; }           // ID único en Firebase
    public string? Barcode { get; set; }      // Código de barras
    public string? ProductName { get; set; }  // Nombre del producto
    public int Quantity { get; set; }         // Cantidad
    public double Latitude { get; set; }      // Latitud GPS
    public double Longitude { get; set; }     // Longitud GPS
    public double? Accuracy { get; set; }     // Precisión GPS
    public DateTime ScanDate { get; set; }    // Fecha de escaneo
    public string? UserId { get; set; }       // ID del usuario
    public string? UserEmail { get; set; }    // Email del usuario
    public string? LocationId { get; set; }   // ID de la ubicación asociada
    public string? LocationName { get; set; } // Nombre de la ubicación
}
```

### StorageLocation

Representa una ubicación o almacén donde se guardan productos.

```csharp
public class StorageLocation
{
    public string? Id { get; set; }           // ID único en Firebase
    public string? Name { get; set; }         // Nombre de la ubicación
    public string? Description { get; set; }  // Descripción
    public string? Address { get; set; }      // Dirección física
    public double Latitude { get; set; }      // Latitud GPS
    public double Longitude { get; set; }     // Longitud GPS
    public string? IconEmoji { get; set; }    // Icono (emoji)
    public string? Color { get; set; }        // Color de identificación
    public DateTime CreatedAt { get; set; }   // Fecha de creación
    public string? UserId { get; set; }       // ID del usuario
    public int ProductCount { get; set; }     // Cantidad de productos
}
```

---

## Servicios

### FirebaseDatabaseService

Servicio para operaciones CRUD con Firebase Realtime Database.

#### Métodos para Productos

| Método | Descripción | Retorno |
|--------|-------------|---------|
| `AddProductScanAsync(ProductScan)` | Agrega un nuevo producto | `Task<string?>` (ID) |
| `GetAllProductScansAsync()` | Obtiene todos los productos del usuario | `Task<List<ProductScan>>` |
| `GetProductScanByIdAsync(string)` | Obtiene un producto por ID | `Task<ProductScan?>` |
| `UpdateProductScanAsync(ProductScan)` | Actualiza un producto | `Task<bool>` |
| `DeleteProductScanAsync(string)` | Elimina un producto | `Task<bool>` |
| `SearchByBarcodeAsync(string)` | Busca por código de barras | `Task<List<ProductScan>>` |
| `SearchByNameAsync(string)` | Busca por nombre | `Task<List<ProductScan>>` |

#### Métodos para Ubicaciones

| Método | Descripción | Retorno |
|--------|-------------|---------|
| `AddLocationAsync(StorageLocation)` | Agrega una nueva ubicación | `Task<string?>` (ID) |
| `GetAllLocationsAsync()` | Obtiene todas las ubicaciones | `Task<List<StorageLocation>>` |
| `GetLocationByIdAsync(string)` | Obtiene una ubicación por ID | `Task<StorageLocation?>` |
| `UpdateLocationAsync(StorageLocation)` | Actualiza una ubicación | `Task<bool>` |
| `DeleteLocationAsync(string)` | Elimina una ubicación | `Task<bool>` |
| `GetProductsByLocationAsync(string)` | Productos por ubicación | `Task<List<ProductScan>>` |
| `GetProductCountByLocationAsync(string)` | Cuenta productos | `Task<int>` |

### PreferencesService

Servicio estático para gestionar preferencias locales usando `Microsoft.Maui.Storage.Preferences`.

#### Propiedades Disponibles

| Propiedad | Tipo | Descripción | Default |
|-----------|------|-------------|---------|
| `IsDarkMode` | `bool` | Modo oscuro activado | `false` |
| `AutoScanEnabled` | `bool` | Escaneo automático | `true` |
| `VibrateOnScan` | `bool` | Vibrar al escanear | `true` |
| `SoundOnScan` | `bool` | Sonido al escanear | `true` |
| `CameraFlashEnabled` | `bool` | Flash de cámara | `false` |
| `DefaultQuantity` | `int` | Cantidad por defecto | `1` |
| `ScanCount` | `int` | Total de escaneos | `0` |
| `LastLatitude` | `double` | Última latitud | `0` |
| `LastLongitude` | `double` | Última longitud | `0` |
| `UserEmail` | `string` | Email del usuario | `""` |
| `UserName` | `string` | Nombre del usuario | `""` |
| `IsFirstLaunch` | `bool` | Primera ejecución | `true` |
| `LastSyncDate` | `DateTime` | Última sincronización | `MinValue` |

#### Métodos

```csharp
IncrementScanCount()           // Incrementa contador de escaneos
SaveLastLocation(lat, lng)     // Guarda última ubicación GPS
GetLastLocation()              // Obtiene última ubicación (lat, lng)
ClearAllPreferences()          // Borra todas las preferencias
ResetToDefaults()              // Restaura valores por defecto
ClearUserInfo()                // Limpia info del usuario (logout)
UpdateLastSyncDate()           // Actualiza fecha de sincronización
```

---

## Páginas de la Aplicación

### 1. SplashPage (Pantalla de Carga)

- Muestra el logo de LocaScan
- Verifica si hay sesión activa
- Redirige a Login o Inventario según corresponda

### 2. MainPage (Login)

- Autenticación con Firebase Auth
- Campos: Email y Contraseña
- Opciones: Iniciar sesión / Registrarse
- Diseño Material Design 3

### 3. InventoryPage (Inventario) - **Pantalla Principal**

**Funcionalidades:**
- Escaneo de códigos de barras con cámara
- Entrada manual de código de barras
- Nombre del producto
- Control de cantidad (+/-)
- Selector de ubicación/almacén
- Obtención de coordenadas GPS
- Lista de productos con swipe para editar/eliminar
- Estadísticas: Total productos y unidades

**Navegación:**
- ⚙️ → Configuración
- 🚪 → Cerrar sesión
- ➕ → Gestionar ubicaciones

### 4. LocationsPage (Ubicaciones)

**Funcionalidades:**
- Crear nuevas ubicaciones/almacenes
- Nombre, descripción y dirección
- Coordenadas GPS
- Selector de icono (🏭 🏪 📦 🏠 🏢)
- Lista de ubicaciones con contador de productos
- Swipe para editar/eliminar

### 5. SettingsPage (Configuración)

**Secciones:**
- 🎨 **Apariencia**: Modo oscuro
- 📊 **Estadísticas**: Total escaneos, última sincronización, última ubicación
- 👤 **Usuario**: Email y nombre

**Acciones:**
- ℹ️ Información del desarrollador (con opción de enviar correo)
- 🔄 Restaurar valores por defecto
- 🗑️ Borrar todos los datos

### 6. HomePage (Legacy)

Página original con funciones de escaneo básicas. Mantenida para compatibilidad.

---

## Base de Datos Firebase

### Estructura de Datos

```
firebase-realtime-database/
├── product_scans/
│   └── {userId}/
│       └── {productId}/
│           ├── Barcode: "7501234567890"
│           ├── ProductName: "Producto Ejemplo"
│           ├── Quantity: 5
│           ├── Latitude: 19.4326
│           ├── Longitude: -99.1332
│           ├── Accuracy: 10.5
│           ├── ScanDate: "2025-12-03T10:30:00Z"
│           ├── UserId: "abc123"
│           ├── UserEmail: "user@email.com"
│           ├── LocationId: "loc456"
│           └── LocationName: "Bodega Principal"
│
└── locations/
    └── {userId}/
        └── {locationId}/
            ├── Name: "Bodega Principal"
            ├── Description: "Almacén central"
            ├── Address: "Calle 123, Ciudad"
            ├── Latitude: 19.4326
            ├── Longitude: -99.1332
            ├── IconEmoji: "🏭"
            ├── Color: "#6750A4"
            ├── CreatedAt: "2025-12-03T10:00:00Z"
            ├── UserId: "abc123"
            └── ProductCount: 15
```

### URL de Firebase

```
https://pitest-cddce-default-rtdb.firebaseio.com/
```

---

## Configuración y Preferencias

### Almacenamiento Local

Las preferencias se almacenan localmente usando `Microsoft.Maui.Storage.Preferences`:

```csharp
// Ejemplo de uso
PreferencesService.IsDarkMode = true;
PreferencesService.IncrementScanCount();
var (lat, lng) = PreferencesService.GetLastLocation();
```

### Claves de Preferencias

| Clave | Descripción |
|-------|-------------|
| `dark_mode` | Modo oscuro |
| `auto_scan` | Escaneo automático |
| `vibrate_on_scan` | Vibración |
| `sound_on_scan` | Sonido |
| `camera_flash` | Flash |
| `default_quantity` | Cantidad default |
| `scan_count` | Contador escaneos |
| `last_latitude` | Última latitud |
| `last_longitude` | Última longitud |
| `user_email` | Email usuario |
| `user_name` | Nombre usuario |
| `first_launch` | Primera ejecución |
| `last_sync_date` | Última sincronización |

---

## Funcionalidades Principales

### 1. Escaneo de Códigos de Barras

```csharp
// Configuración del escáner
var options = new BarcodeReaderOptions
{
    Formats = BarcodeFormats.All,
    AutoRotate = true,
    Multiple = false,
    TryHarder = true
};
```

**Formatos soportados:**
- EAN-13, EAN-8
- UPC-A, UPC-E
- Code 128, Code 39
- QR Code
- Data Matrix
- Y más...

### 2. Geolocalización

```csharp
var request = new GeolocationRequest(
    GeolocationAccuracy.High, 
    TimeSpan.FromSeconds(10)
);
var location = await Geolocation.Default.GetLocationAsync(request);
```

### 3. Autenticación Firebase

```csharp
// Login
await CrossFirebaseAuth.Current.SignInWithEmailAndPasswordAsync(email, password);

// Registro
await CrossFirebaseAuth.Current.CreateUserWithEmailAndPasswordAsync(email, password);

// Logout
await CrossFirebaseAuth.Current.SignOutAsync();

// Usuario actual
var user = CrossFirebaseAuth.Current.CurrentUser;
```

---

## Permisos Requeridos

### Android (AndroidManifest.xml)

```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.VIBRATE" />
```

---

## Instalación y Configuración

### Requisitos Previos

1. Visual Studio 2022 con carga de trabajo .NET MAUI
2. .NET 9.0 SDK
3. Android SDK (API 21+)
4. Cuenta de Firebase

### Pasos de Instalación

1. **Clonar el repositorio**
   ```bash
   git clone [url-del-repositorio]
   cd apitest
   ```

2. **Configurar Firebase**
   - Crear proyecto en Firebase Console
   - Habilitar Authentication (Email/Password)
   - Crear Realtime Database
   - Descargar `google-services.json`
   - Colocar en `Platforms/Android/`

3. **Restaurar paquetes**
   ```bash
   dotnet restore
   ```

4. **Compilar y ejecutar**
   ```bash
   dotnet build
   dotnet run
   ```

### Configuración de Firebase

Actualizar la URL de Firebase en `FirebaseDatabaseService.cs`:

```csharp
private const string FirebaseUrl = "https://tu-proyecto.firebaseio.com/";
```

---

## Paleta de Colores (Material Design 3)

| Color | Hex | Uso |
|-------|-----|-----|
| Primary | `#6750A4` | Botones principales, acentos |
| OnPrimary | `#FFFFFF` | Texto sobre primary |
| Secondary | `#625B71` | Botones secundarios |
| Surface | `#FFFBFE` | Fondos |
| Error | `#BA1A1A` | Errores, eliminar |
| Background | `#FAFAFA` | Fondo de página |

---

## Información del Desarrollador

**Empresa:** Los Jochis Solutions

**Contacto:** jochis@gmail.com

**Versión:** 1.0

**Funcionalidades implementadas:**
- ✅ Autenticación con Firebase
- ✅ Escaneo de códigos de barras y QR
- ✅ Geolocalización GPS
- ✅ Almacenamiento en Realtime Database
- ✅ Interfaz Material Design 3
- ✅ Gestión de ubicaciones/almacenes
- ✅ CRUD completo de productos
- ✅ Preferencias locales
- ✅ Modo oscuro

---

## Changelog

### v1.0 (Diciembre 2025)
- Lanzamiento inicial
- Sistema de autenticación
- Escaneo de códigos de barras
- Geolocalización
- CRUD de productos
- CRUD de ubicaciones
- Configuración de preferencias
- Interfaz Material Design 3

---

*Documentación generada para LocaScan - Diciembre 2025*

