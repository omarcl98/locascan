using System.Collections.ObjectModel;
using apitest.Models;
using apitest.Services;
using Plugin.Firebase.Auth;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace apitest
{
    public partial class InventoryPage : ContentPage
    {
        private readonly FirebaseDatabaseService _firebaseService;
        private ObservableCollection<ProductScan> _products;
        private List<StorageLocation> _locations;
        private StorageLocation? _selectedLocation;
        private Location? _currentGpsLocation;
        private ProductScan? _editingProduct;
        private bool _isEditing;
        private CameraBarcodeReaderView? _barcodeReader;

        public ObservableCollection<ProductScan> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public InventoryPage()
        {
            InitializeComponent();
            _firebaseService = new FirebaseDatabaseService();
            _products = new ObservableCollection<ProductScan>();
            _locations = new List<StorageLocation>();
            BindingContext = this;
            
            // Suscribirse al evento Loaded para forzar colores
            this.Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object? sender, EventArgs e)
        {
            ForceEntryColors();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Asegurar que el escáner esté cerrado al entrar a la página
            CloseScannerView();
            
            // Cargar cantidad por defecto desde preferencias
            QuantityEntry.Text = PreferencesService.DefaultQuantity.ToString();
            
            await LoadLocationsAsync();
            await LoadProductsAsync();
            await GetCurrentLocationAsync();
            
            // Forzar colores de los Entry después de que todo esté cargado
            await Task.Delay(100);
            ForceEntryColors();
        }

        private void ForceEntryColors()
        {
            // Forzar colores oscuros en los campos de texto usando Handler
            var blackColor = Colors.Black;
            var whiteColor = Colors.White;
            var grayColor = Colors.Gray;

            // Aplicar a BarcodeEntry
            BarcodeEntry.TextColor = blackColor;
            BarcodeEntry.BackgroundColor = whiteColor;
            BarcodeEntry.PlaceholderColor = grayColor;
            
            // Aplicar a ProductNameEntry
            ProductNameEntry.TextColor = blackColor;
            ProductNameEntry.BackgroundColor = whiteColor;
            ProductNameEntry.PlaceholderColor = grayColor;
            
            // Aplicar a QuantityEntry
            QuantityEntry.TextColor = blackColor;
            QuantityEntry.BackgroundColor = whiteColor;
            QuantityEntry.PlaceholderColor = grayColor;

#if ANDROID
            // Forzar colores a nivel nativo en Android
            ApplyAndroidEntryColors(BarcodeEntry);
            ApplyAndroidEntryColors(ProductNameEntry);
            ApplyAndroidEntryColors(QuantityEntry);
#endif
        }

#if ANDROID
        private void ApplyAndroidEntryColors(Entry entry)
        {
            entry.Handler?.UpdateValue(nameof(Entry.TextColor));
            entry.Handler?.UpdateValue(nameof(Entry.BackgroundColor));
            
            if (entry.Handler?.PlatformView is Android.Widget.EditText editText)
            {
                editText.SetTextColor(Android.Graphics.Color.Black);
                editText.SetBackgroundColor(Android.Graphics.Color.White);
                editText.SetHintTextColor(Android.Graphics.Color.Gray);
            }
        }
#endif

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Destruir la cámara cuando se sale de la página
            CloseScannerView();
        }

        private void CreateBarcodeReader()
        {
            // Eliminar el lector anterior si existe
            if (_barcodeReader != null)
            {
                _barcodeReader.BarcodesDetected -= OnBarcodesDetected;
                CameraContainer.Children.Clear();
                _barcodeReader = null;
            }

            // Crear nuevo lector de códigos de barras
            _barcodeReader = new CameraBarcodeReaderView
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Options = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormats.All,
                    AutoRotate = true,
                    Multiple = false,
                    TryHarder = true
                }
            };
            
            _barcodeReader.BarcodesDetected += OnBarcodesDetected;
            CameraContainer.Children.Add(_barcodeReader);
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var products = await _firebaseService.GetAllProductScansAsync();
                
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
                
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar productos: {ex.Message}", "OK");
            }
        }

        private void UpdateStatistics()
        {
            TotalProductsLabel.Text = Products.Count.ToString();
            TotalQuantityLabel.Text = Products.Sum(p => p.Quantity).ToString();
        }

        private async Task LoadLocationsAsync()
        {
            try
            {
                _locations = await _firebaseService.GetAllLocationsAsync();
                
                // Agregar opción "Sin ubicación" al inicio
                LocationPicker.Items.Clear();
                LocationPicker.Items.Add("Sin ubicación");
                
                foreach (var location in _locations)
                {
                    LocationPicker.Items.Add($"{location.IconEmoji} {location.Name}");
                }
                
                LocationPicker.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar ubicaciones: {ex.Message}");
            }
        }

        private void OnLocationPickerChanged(object? sender, EventArgs e)
        {
            var index = LocationPicker.SelectedIndex;
            if (index > 0 && index <= _locations.Count)
            {
                _selectedLocation = _locations[index - 1];
            }
            else
            {
                _selectedLocation = null;
            }
        }

        private async void OnManageLocationsClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LocationsPage");
        }

        private async Task GetCurrentLocationAsync()
        {
            try
            {
                LocationButton.IsEnabled = false;
                LocationLabel.Text = "Obteniendo...";

                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                _currentGpsLocation = await Geolocation.Default.GetLocationAsync(request);

                if (_currentGpsLocation != null)
                {
                    LocationLabel.Text = $"{_currentGpsLocation.Latitude:N6}, {_currentGpsLocation.Longitude:N6}";
                    
                    // Guardar última ubicación en preferencias
                    PreferencesService.SaveLastLocation(_currentGpsLocation.Latitude, _currentGpsLocation.Longitude);
                }
                else
                {
                    // Intentar usar última ubicación guardada
                    if (PreferencesService.HasSavedLocation)
                    {
                        var (lat, lng) = PreferencesService.GetLastLocation();
                        _currentGpsLocation = new Location(lat, lng);
                        LocationLabel.Text = $"{lat:N6}, {lng:N6} (guardada)";
                    }
                    else
                    {
                        LocationLabel.Text = "No disponible";
                    }
                }
            }
            catch (Exception ex)
            {
                LocationLabel.Text = "Error al obtener ubicación";
                Console.WriteLine($"Error de ubicación: {ex.Message}");
                
                // Intentar usar última ubicación guardada
                if (PreferencesService.HasSavedLocation)
                {
                    var (lat, lng) = PreferencesService.GetLastLocation();
                    _currentGpsLocation = new Location(lat, lng);
                    LocationLabel.Text = $"{lat:N6}, {lng:N6} (guardada)";
                }
            }
            finally
            {
                LocationButton.IsEnabled = true;
            }
        }

        private async void OnScanClicked(object? sender, EventArgs e)
        {
            await CheckAndRequestCameraPermission();
        }

        private async Task CheckAndRequestCameraPermission()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                }

                if (status == PermissionStatus.Granted)
                {
                    // Crear nuevo lector de cámara cada vez
                    CreateBarcodeReader();
                    
                    ScannerOverlay.IsVisible = true;
                    
                    // Esperar un momento para que la cámara se inicialice
                    await Task.Delay(100);
                    
                    if (_barcodeReader != null)
                    {
                        _barcodeReader.IsDetecting = true;
                    }
                }
                else
                {
                    await DisplayAlert("Permiso Denegado", 
                        "Se necesita acceso a la cámara para escanear códigos de barras.\n\n" +
                        "Por favor, ve a Configuración > Aplicaciones > LocaScan > Permisos y activa la cámara.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al acceder a la cámara: {ex.Message}", "OK");
            }
        }

        private void OnCloseScannerClicked(object? sender, EventArgs e)
        {
            CloseScannerView();
        }

        private void CloseScannerView()
        {
            // Detener y destruir la cámara
            if (_barcodeReader != null)
            {
                _barcodeReader.IsDetecting = false;
                _barcodeReader.BarcodesDetected -= OnBarcodesDetected;
                CameraContainer.Children.Clear();
                _barcodeReader = null;
            }
            
            ScannerOverlay.IsVisible = false;
        }

        private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var first = e.Results?.FirstOrDefault();
            if (first != null)
            {
                Dispatcher.Dispatch(() =>
                {
                    // Cerrar escáner primero
                    CloseScannerView();
                    
                    // Asignar el código escaneado
                    BarcodeEntry.Text = first.Value;
                    
                    // Incrementar contador de escaneos
                    PreferencesService.IncrementScanCount();
                    
                    // Vibrar si está activado
                    if (PreferencesService.VibrateOnScan)
                    {
                        try
                        {
                            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
                        }
                        catch { }
                    }
                    
                    // Enfocar en el nombre del producto
                    ProductNameEntry.Focus();
                });
            }
        }

        private async void OnGetLocationClicked(object? sender, EventArgs e)
        {
            await GetCurrentLocationAsync();
        }

        private void OnIncreaseQuantity(object? sender, EventArgs e)
        {
            if (int.TryParse(QuantityEntry.Text, out int quantity))
            {
                QuantityEntry.Text = (quantity + 1).ToString();
            }
            else
            {
                QuantityEntry.Text = "1";
            }
        }

        private void OnDecreaseQuantity(object? sender, EventArgs e)
        {
            if (int.TryParse(QuantityEntry.Text, out int quantity) && quantity > 1)
            {
                QuantityEntry.Text = (quantity - 1).ToString();
            }
            else
            {
                QuantityEntry.Text = "1";
            }
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(BarcodeEntry.Text))
                {
                    await DisplayAlert("Error", "Por favor escanea o ingresa un código de barras", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(ProductNameEntry.Text))
                {
                    await DisplayAlert("Error", "Por favor ingresa el nombre del producto", "OK");
                    return;
                }

                if (!int.TryParse(QuantityEntry.Text, out int quantity) || quantity < 1)
                {
                    await DisplayAlert("Error", "Por favor ingresa una cantidad válida", "OK");
                    return;
                }

                SaveButton.IsEnabled = false;
                SaveButton.Text = "⏳ GUARDANDO...";

                var product = new ProductScan
                {
                    Barcode = BarcodeEntry.Text.Trim(),
                    ProductName = ProductNameEntry.Text.Trim(),
                    Quantity = quantity,
                    Latitude = _currentGpsLocation?.Latitude ?? 0,
                    Longitude = _currentGpsLocation?.Longitude ?? 0,
                    Accuracy = _currentGpsLocation?.Accuracy,
                    LocationId = _selectedLocation?.Id,
                    LocationName = _selectedLocation?.Name
                };

                if (_isEditing && _editingProduct != null)
                {
                    // Actualizar producto existente
                    product.Id = _editingProduct.Id;
                    product.ScanDate = _editingProduct.ScanDate;
                    
                    var success = await _firebaseService.UpdateProductScanAsync(product);
                    if (success)
                    {
                        await DisplayAlert("Éxito", "✅ Producto actualizado correctamente", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo actualizar el producto", "OK");
                    }
                    
                    _isEditing = false;
                    _editingProduct = null;
                }
                else
                {
                    // Crear nuevo producto
                    var productId = await _firebaseService.AddProductScanAsync(product);
                    if (!string.IsNullOrEmpty(productId))
                    {
                        await DisplayAlert("Éxito", "✅ Producto guardado correctamente", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo guardar el producto", "OK");
                    }
                }

                // Limpiar formulario
                ClearForm();
                
                // Recargar lista
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
            }
            finally
            {
                SaveButton.IsEnabled = true;
                SaveButton.Text = _isEditing ? "💾 ACTUALIZAR PRODUCTO" : "💾 GUARDAR PRODUCTO";
            }
        }

        private void ClearForm()
        {
            BarcodeEntry.Text = string.Empty;
            ProductNameEntry.Text = string.Empty;
            QuantityEntry.Text = PreferencesService.DefaultQuantity.ToString();
            LocationPicker.SelectedIndex = 0;
            _selectedLocation = null;
            _isEditing = false;
            _editingProduct = null;
            SaveButton.Text = "💾 GUARDAR PRODUCTO";
            
            // Actualizar fecha de sincronización
            PreferencesService.UpdateLastSyncDate();
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await LoadProductsAsync();
            ProductsRefreshView.IsRefreshing = false;
        }

        private void OnEditSwipe(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is ProductScan product)
            {
                EditProduct(product);
            }
        }

        private void EditProduct(ProductScan product)
        {
            _isEditing = true;
            _editingProduct = product;
            
            BarcodeEntry.Text = product.Barcode;
            ProductNameEntry.Text = product.ProductName;
            QuantityEntry.Text = product.Quantity.ToString();
            
            // Seleccionar ubicación si existe
            if (!string.IsNullOrEmpty(product.LocationId))
            {
                var locationIndex = _locations.FindIndex(l => l.Id == product.LocationId);
                if (locationIndex >= 0)
                {
                    LocationPicker.SelectedIndex = locationIndex + 1; // +1 por "Sin ubicación"
                    _selectedLocation = _locations[locationIndex];
                }
            }
            else
            {
                LocationPicker.SelectedIndex = 0;
                _selectedLocation = null;
            }
            
            if (product.Latitude != 0 && product.Longitude != 0)
            {
                _currentGpsLocation = new Location(product.Latitude, product.Longitude);
                LocationLabel.Text = $"{product.Latitude:N6}, {product.Longitude:N6}";
            }
            
            SaveButton.Text = "💾 ACTUALIZAR PRODUCTO";
            
            // Scroll hacia arriba para ver el formulario
            // y dar feedback visual
            DisplayAlert("Editar", $"Editando: {product.ProductName}", "OK");
        }

        private async void OnDeleteSwipe(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is ProductScan product)
            {
                await DeleteProduct(product);
            }
        }

        private async Task DeleteProduct(ProductScan product)
        {
            var confirm = await DisplayAlert(
                "Confirmar eliminación",
                $"¿Estás seguro de eliminar '{product.ProductName}'?",
                "Eliminar",
                "Cancelar");

            if (confirm)
            {
                try
                {
                    var success = await _firebaseService.DeleteProductScanAsync(product.Id!);
                    if (success)
                    {
                        Products.Remove(product);
                        UpdateStatistics();
                        await DisplayAlert("Éxito", "✅ Producto eliminado", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo eliminar el producto", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Error al eliminar: {ex.Message}", "OK");
                }
            }
        }

        private async void OnSettingsClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//SettingsPage");
        }

        private async void OnLogoutClicked(object? sender, EventArgs e)
        {
            var confirm = await DisplayAlert(
                "Cerrar sesión",
                "¿Estás seguro de que deseas cerrar sesión?",
                "Sí, cerrar sesión",
                "Cancelar");

            if (confirm)
            {
                try
                {
                    var firebaseAuth = CrossFirebaseAuth.Current;
                    await firebaseAuth.SignOutAsync();
                    
                    // Limpiar información del usuario en preferencias
                    PreferencesService.ClearUserInfo();
                    
                    await Shell.Current.GoToAsync("//MainPage");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Error al cerrar sesión: {ex.Message}", "OK");
                }
            }
        }
    }
}

