using Firebase.Database;
using Firebase.Database.Query;
using apitest.Models;
using Plugin.Firebase.Auth;

namespace apitest.Services
{
    public class FirebaseDatabaseService
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly IFirebaseAuth _firebaseAuth;
        private const string FirebaseUrl = "https://pitest-cddce-default-rtdb.firebaseio.com/";

        public FirebaseDatabaseService()
        {
            _firebaseClient = new FirebaseClient(FirebaseUrl);
            _firebaseAuth = CrossFirebaseAuth.Current;
        }

        private string? GetCurrentUserId()
        {
            return _firebaseAuth.CurrentUser?.Uid;
        }

        private string? GetCurrentUserEmail()
        {
            return _firebaseAuth.CurrentUser?.Email;
        }

        // CREATE - Agregar nuevo producto escaneado
        public async Task<string?> AddProductScanAsync(ProductScan product)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("Usuario no autenticado");

                product.UserId = userId;
                product.UserEmail = GetCurrentUserEmail();
                product.ScanDate = DateTime.UtcNow;

                var result = await _firebaseClient
                    .Child("product_scans")
                    .Child(userId)
                    .PostAsync(product);

                return result.Key;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar producto: {ex.Message}");
                throw;
            }
        }

        // READ - Obtener todos los productos del usuario actual
        public async Task<List<ProductScan>> GetAllProductScansAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return new List<ProductScan>();

                var products = await _firebaseClient
                    .Child("product_scans")
                    .Child(userId)
                    .OnceAsync<ProductScan>();

                return products
                    .Select(p =>
                    {
                        var product = p.Object;
                        product.Id = p.Key;
                        return product;
                    })
                    .OrderByDescending(p => p.ScanDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener productos: {ex.Message}");
                return new List<ProductScan>();
            }
        }

        // READ - Obtener un producto por ID
        public async Task<ProductScan?> GetProductScanByIdAsync(string productId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return null;

                var product = await _firebaseClient
                    .Child("product_scans")
                    .Child(userId)
                    .Child(productId)
                    .OnceSingleAsync<ProductScan>();

                if (product != null)
                    product.Id = productId;

                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener producto: {ex.Message}");
                return null;
            }
        }

        // UPDATE - Actualizar producto existente
        public async Task<bool> UpdateProductScanAsync(ProductScan product)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(product.Id))
                    return false;

                await _firebaseClient
                    .Child("product_scans")
                    .Child(userId)
                    .Child(product.Id)
                    .PutAsync(product);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar producto: {ex.Message}");
                return false;
            }
        }

        // DELETE - Eliminar producto
        public async Task<bool> DeleteProductScanAsync(string productId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return false;

                await _firebaseClient
                    .Child("product_scans")
                    .Child(userId)
                    .Child(productId)
                    .DeleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar producto: {ex.Message}");
                return false;
            }
        }

        // Buscar productos por código de barras
        public async Task<List<ProductScan>> SearchByBarcodeAsync(string barcode)
        {
            try
            {
                var allProducts = await GetAllProductScansAsync();
                return allProducts
                    .Where(p => p.Barcode?.Contains(barcode, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar por código: {ex.Message}");
                return new List<ProductScan>();
            }
        }

        // Buscar productos por nombre
        public async Task<List<ProductScan>> SearchByNameAsync(string name)
        {
            try
            {
                var allProducts = await GetAllProductScansAsync();
                return allProducts
                    .Where(p => p.ProductName?.Contains(name, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar por nombre: {ex.Message}");
                return new List<ProductScan>();
            }
        }

        #region Ubicaciones / Locations

        // CREATE - Agregar nueva ubicación
        public async Task<string?> AddLocationAsync(StorageLocation location)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("Usuario no autenticado");

                location.UserId = userId;
                location.CreatedAt = DateTime.UtcNow;

                var result = await _firebaseClient
                    .Child("locations")
                    .Child(userId)
                    .PostAsync(location);

                return result.Key;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar ubicación: {ex.Message}");
                throw;
            }
        }

        // READ - Obtener todas las ubicaciones del usuario
        public async Task<List<StorageLocation>> GetAllLocationsAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return new List<StorageLocation>();

                var locations = await _firebaseClient
                    .Child("locations")
                    .Child(userId)
                    .OnceAsync<StorageLocation>();

                return locations
                    .Select(l =>
                    {
                        var location = l.Object;
                        location.Id = l.Key;
                        return location;
                    })
                    .OrderBy(l => l.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ubicaciones: {ex.Message}");
                return new List<StorageLocation>();
            }
        }

        // READ - Obtener una ubicación por ID
        public async Task<StorageLocation?> GetLocationByIdAsync(string locationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return null;

                var location = await _firebaseClient
                    .Child("locations")
                    .Child(userId)
                    .Child(locationId)
                    .OnceSingleAsync<StorageLocation>();

                if (location != null)
                    location.Id = locationId;

                return location;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ubicación: {ex.Message}");
                return null;
            }
        }

        // UPDATE - Actualizar ubicación existente
        public async Task<bool> UpdateLocationAsync(StorageLocation location)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(location.Id))
                    return false;

                await _firebaseClient
                    .Child("locations")
                    .Child(userId)
                    .Child(location.Id)
                    .PutAsync(location);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar ubicación: {ex.Message}");
                return false;
            }
        }

        // DELETE - Eliminar ubicación
        public async Task<bool> DeleteLocationAsync(string locationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return false;

                await _firebaseClient
                    .Child("locations")
                    .Child(userId)
                    .Child(locationId)
                    .DeleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar ubicación: {ex.Message}");
                return false;
            }
        }

        // Obtener productos por ubicación
        public async Task<List<ProductScan>> GetProductsByLocationAsync(string locationId)
        {
            try
            {
                var allProducts = await GetAllProductScansAsync();
                return allProducts
                    .Where(p => p.LocationId == locationId)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener productos por ubicación: {ex.Message}");
                return new List<ProductScan>();
            }
        }

        // Contar productos por ubicación
        public async Task<int> GetProductCountByLocationAsync(string locationId)
        {
            var products = await GetProductsByLocationAsync(locationId);
            return products.Count;
        }

        #endregion
    }
}

