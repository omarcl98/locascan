using Plugin.Firebase.Auth;
using ZXing.Net.Maui.Controls;
using ZXing.Net.Maui;
using Microsoft.Maui.ApplicationModel;

namespace apitest
{
    public partial class HomePage : ContentPage
    {
        private IFirebaseAuth _firebaseAuth;
        private bool _isCameraActive = false;
        private Location? _currentLocation = null;

        public HomePage()
        {
            InitializeComponent();
            _firebaseAuth = CrossFirebaseAuth.Current;
            LoadUserInfo();
            SetupBarcodeReader();
            UpdateToggleButton(); // Inicializar el estado del botón
        }

        private void LoadUserInfo()
        {
            try
            {
                var currentUser = _firebaseAuth.CurrentUser;
                if (currentUser != null)
                {
                    var userName = currentUser.DisplayName ?? currentUser.Email?.Split('@')[0] ?? "Usuario";
                    var userEmail = currentUser.Email ?? "";
                    
                    UserInfoLabel.Text = $"Usuario: {userName}\nEmail: {userEmail}";
                }
            }
            catch
            {
                UserInfoLabel.Text = "Información del usuario no disponible";
            }
        }

        private void SetupBarcodeReader()
        {
            barcodeView.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.All,
                AutoRotate = true,
                Multiple = false
            };
        }

        protected void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            foreach (var barcode in e.Results)
            {
                Console.WriteLine($"Código detectado: {barcode.Format} -> {barcode.Value}");
            }

            var first = e.Results?.FirstOrDefault();
            if (first is not null)
            {
                Dispatcher.Dispatch(async () =>
                {
                    // Obtener ubicación actual si no la tenemos
                    if (_currentLocation == null)
                    {
                        await GetCurrentLocationAsync();
                    }

                    // Mostrar el resultado del escaneo con ubicación
                    string locationInfo = _currentLocation != null 
                        ? $"\n📍 Ubicación: {_currentLocation.Latitude:N6}, {_currentLocation.Longitude:N6}"
                        : "\n📍 Ubicación: No disponible";

                    ResultLabel.Text = $"✅ Código detectado:\nTipo: {first.Format}\nContenido: {first.Value}{locationInfo}";
                    ResultLabel.TextColor = Application.Current?.Resources["Primary"] as Color ?? Colors.Green;
                    ResultFrame.IsVisible = true;

                    // Mostrar alerta con el resultado completo
                    string alertMessage = $"Tipo: {first.Format}\n\nContenido:\n{first.Value}";
                    if (_currentLocation != null)
                    {
                        alertMessage += $"\n\n📍 Ubicación del escaneo:\n" +
                                      $"Latitud: {_currentLocation.Latitude:N6}\n" +
                                      $"Longitud: {_currentLocation.Longitude:N6}\n" +
                                      $"Precisión: {(_currentLocation.Accuracy?.ToString("N2") ?? "N/A")} metros";
                    }
                    else
                    {
                        alertMessage += "\n\n📍 Ubicación: No disponible";
                    }

                    await DisplayAlert("Código Escaneado", alertMessage, "OK");

                    // Opcional: Desactivar la cámara después del escaneo
                    // barcodeView.IsDetecting = false;
                    // _isCameraActive = false;
                    // UpdateToggleButton();
                });
            }
        }

        private async void OnToggleCameraClicked(object? sender, EventArgs e)
        {
            try
            {
                ToggleCameraButton.IsEnabled = false;
                
                if (!_isCameraActive)
                {
                    // Activar cámara
                    await CheckAndRequestCameraPermission();
                }
                else
                {
                    // Desactivar cámara
                    barcodeView.IsDetecting = false;
                    _isCameraActive = false;
                    UpdateToggleButton();
                    ResultLabel.Text = "Cámara desactivada - Haz clic en 'Cámara' para activar";
                    ResultLabel.TextColor = Application.Current?.Resources["OnSurfaceVariant"] as Color ?? Colors.Gray;
                    ResultFrame.IsVisible = true;
                }
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("Error", $"Error al controlar la cámara: {ex.Message}", "OK");
            }
            finally
            {
                ToggleCameraButton.IsEnabled = true;
            }
        }

        public async Task CheckAndRequestCameraPermission()
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
                    barcodeView.IsDetecting = true;
                    _isCameraActive = true;
                    UpdateToggleButton();
                    ResultLabel.Text = "Cámara activada - Escanea un código";
                    ResultLabel.TextColor = Application.Current?.Resources["Primary"] as Color ?? Colors.Blue;
                    ResultFrame.IsVisible = true;
                    
                    await DisplayAlert("Cámara Activada", "La cámara está lista para escanear códigos de barras y códigos QR.", "OK");
                }
                else
                {
                    await DisplayAlert("Permiso Denegado", 
                        "Se necesita acceso a la cámara para escanear códigos de barras y códigos QR.\n\n" +
                        "Por favor, ve a Configuración > Aplicaciones > [Tu App] > Permisos y activa la cámara.", "OK");
                    
                    _isCameraActive = false;
                    UpdateToggleButton();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al acceder a la cámara: {ex.Message}", "OK");
                _isCameraActive = false;
                UpdateToggleButton();
            }
        }

        private void UpdateToggleButton()
        {
            if (_isCameraActive)
            {
                ToggleCameraButton.Text = "⏸️ Cámara";
                ToggleCameraButton.BackgroundColor = Colors.Orange;
            }
            else
            {
                ToggleCameraButton.Text = "▶️ Cámara";
                ToggleCameraButton.BackgroundColor = Colors.Green;
            }
        }

        private async void OnGetLocationClicked(object? sender, EventArgs e)
        {
            await GetCurrentLocationAsync();
        }

        private async Task GetCurrentLocationAsync()
        {
            try
            {
                GetLocationButton.IsEnabled = false;
                GetLocationButton.Text = "📍 Obteniendo...";

                // Crear una solicitud de geolocalización con alta precisión
                GeolocationRequest request = new GeolocationRequest(
                    GeolocationAccuracy.High,
                    TimeSpan.FromSeconds(10) // Tiempo de espera
                );

                // Obtener la ubicación actual del dispositivo
                Location? location = await Geolocation.Default.GetLocationAsync(request);

                if (location != null)
                {
                    _currentLocation = location;
                    
                    // Actualizar las etiquetas de ubicación
                    LatitudeLabel.Text = $"{location.Latitude:N6}";
                    LongitudeLabel.Text = $"{location.Longitude:N6}";
                    AccuracyLabel.Text = location.Accuracy.HasValue
                        ? $"{location.Accuracy.Value:N2} metros"
                        : "No disponible";

                    await DisplayAlert("Ubicación Obtenida", 
                        $"Latitud: {location.Latitude:N6}\n" +
                        $"Longitud: {location.Longitude:N6}\n" +
                        $"Precisión: {(location.Accuracy?.ToString("N2") ?? "N/A")} metros", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo obtener la ubicación. Asegúrate de tener el GPS activado.", "OK");
                    LatitudeLabel.Text = LongitudeLabel.Text = AccuracyLabel.Text = "No disponible";
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                await DisplayAlert("Error", $"La geolocalización no está soportada en este dispositivo: {fnsEx.Message}", "OK");
                LatitudeLabel.Text = LongitudeLabel.Text = AccuracyLabel.Text = "No soportado";
            }
            catch (PermissionException pEx)
            {
                await DisplayAlert("Permisos", "Se denegó el permiso de ubicación. Por favor, actívalo en la configuración de la aplicación.", "OK");
                LatitudeLabel.Text = LongitudeLabel.Text = AccuracyLabel.Text = "Permiso denegado";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error inesperado: {ex.Message}", "OK");
                LatitudeLabel.Text = LongitudeLabel.Text = AccuracyLabel.Text = "Error";
            }
            finally
            {
                GetLocationButton.IsEnabled = true;
                GetLocationButton.Text = "📍 Obtener Ubicación";
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // No activar automáticamente la cámara al aparecer la página
            // El usuario debe hacer clic en el botón para activarla
        }

        private async void OnInfoClicked(object? sender, EventArgs e)
        {
            var action = await DisplayActionSheet(
                "Información del Desarrollador",
                "Cerrar",
                null,
                "📧 Enviar correo a jochis@gmail.com",
                "ℹ️ Ver información completa"
            );

            if (action == "📧 Enviar correo a jochis@gmail.com")
            {
                await OpenEmailAsync();
            }
            else if (action == "ℹ️ Ver información completa")
            {
                await ShowFullDeveloperInfo();
            }
        }

        private async Task OpenEmailAsync()
        {
            try
            {
                var email = "jochis@gmail.com";
                var subject = Uri.EscapeDataString("Contacto desde LocaScan App");
                var body = Uri.EscapeDataString("Hola,\n\nMe comunico desde la aplicación LocaScan.\n\n");
                
                var uri = new Uri($"mailto:{email}?subject={subject}&body={body}");
                
                await Launcher.Default.OpenAsync(uri);
            }
            catch (Exception ex)
            {
                // Si falla el mailto, intentar copiar el correo al portapapeles
                try
                {
                    await Clipboard.Default.SetTextAsync("jochis@gmail.com");
                    await DisplayAlert("Correo copiado", "El correo jochis@gmail.com ha sido copiado al portapapeles.", "OK");
                }
                catch
                {
                    await DisplayAlert("Error", $"No se pudo abrir el correo: {ex.Message}", "OK");
                }
            }
        }

        private async Task ShowFullDeveloperInfo()
        {
            await DisplayAlert(
                "Información del Desarrollador",
                "🏢 Desarrollado por:\n\n" +
                "Los Jochis Solutions\n\n" +
                "📧 Contacto:\n" +
                "jochis@gmail.com\n\n" +
                "🔧 Funcionalidades:\n" +
                "• Autenticación con Firebase\n" +
                "• Escaneo de códigos de barras y QR\n" +
                "• Geolocalización GPS\n" +
                "• Almacenamiento en Realtime Database\n" +
                "• Interfaz moderna Material Design\n\n" +
                "Versión 1.0",
                "Cerrar"
            );
        }

        private async void OnLogoutClicked(object? sender, EventArgs e)
        {
            try
            {
                // Desactivar cámara antes de cerrar sesión
                if (_isCameraActive)
                {
                    barcodeView.IsDetecting = false;
                    _isCameraActive = false;
                }

                // Cerrar sesión con Firebase
                await _firebaseAuth.SignOutAsync();
                
                // Navegar de vuelta a la página principal
                await Shell.Current.GoToAsync("//MainPage");
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("Error", $"Error al cerrar sesión: {ex.Message}", "OK");
            }
        }

        private async void OnInventoryClicked(object? sender, EventArgs e)
        {
            // Desactivar cámara antes de navegar
            if (_isCameraActive)
            {
                barcodeView.IsDetecting = false;
                _isCameraActive = false;
                UpdateToggleButton();
            }
            
            // Navegar a la página de inventario
            await Shell.Current.GoToAsync("//InventoryPage");
        }
    }
}
