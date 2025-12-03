using apitest.Services;
using Plugin.Firebase.Auth;

namespace apitest
{
    public partial class SettingsPage : ContentPage
    {
        private bool _isLoading = true;

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadSettings();
        }

        private void LoadSettings()
        {
            _isLoading = true;

            // Cargar configuración de apariencia
            DarkModeSwitch.IsToggled = PreferencesService.IsDarkMode;

            // Cargar estadísticas
            ScanCountLabel.Text = PreferencesService.ScanCount.ToString();
            
            var lastSync = PreferencesService.LastSyncDate;
            LastSyncLabel.Text = lastSync == DateTime.MinValue 
                ? "Nunca" 
                : lastSync.ToString("dd/MM/yyyy HH:mm");

            if (PreferencesService.HasSavedLocation)
            {
                var (lat, lng) = PreferencesService.GetLastLocation();
                LastLocationLabel.Text = $"{lat:N4}, {lng:N4}";
            }
            else
            {
                LastLocationLabel.Text = "No disponible";
            }

            // Cargar información del usuario
            LoadUserInfo();

            _isLoading = false;
        }

        private void LoadUserInfo()
        {
            try
            {
                var firebaseAuth = CrossFirebaseAuth.Current;
                var currentUser = firebaseAuth.CurrentUser;

                if (currentUser != null)
                {
                    UserEmailLabel.Text = currentUser.Email ?? "Sin email";
                    UserNameLabel.Text = currentUser.DisplayName ?? currentUser.Email?.Split('@')[0] ?? "";
                    
                    // Guardar en preferences
                    PreferencesService.SaveUserInfo(
                        currentUser.Email ?? "",
                        currentUser.DisplayName ?? currentUser.Email?.Split('@')[0] ?? ""
                    );
                }
                else
                {
                    // Intentar cargar desde preferences
                    var savedEmail = PreferencesService.UserEmail;
                    var savedName = PreferencesService.UserName;

                    if (!string.IsNullOrEmpty(savedEmail))
                    {
                        UserEmailLabel.Text = savedEmail;
                        UserNameLabel.Text = savedName;
                    }
                    else
                    {
                        UserEmailLabel.Text = "No conectado";
                        UserNameLabel.Text = "";
                    }
                }
            }
            catch
            {
                UserEmailLabel.Text = "Error al cargar usuario";
                UserNameLabel.Text = "";
            }
        }

        #region Event Handlers - Apariencia

        private void OnDarkModeToggled(object? sender, ToggledEventArgs e)
        {
            if (_isLoading) return;
            PreferencesService.IsDarkMode = e.Value;
            
            // Aplicar tema (esto requeriría implementación adicional en App.xaml.cs)
            Application.Current!.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
            
            ShowToast($"Modo oscuro: {(e.Value ? "Activado" : "Desactivado")}");
        }

        #endregion

        #region Event Handlers - Acciones

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

        private async void OnResetClicked(object? sender, EventArgs e)
        {
            var confirm = await DisplayAlert(
                "Restaurar valores",
                "¿Estás seguro de restaurar todos los ajustes a sus valores por defecto?",
                "Restaurar",
                "Cancelar");

            if (confirm)
            {
                // Restaurar valores por defecto
                PreferencesService.IsDarkMode = false;

                // Aplicar tema claro
                Application.Current!.UserAppTheme = AppTheme.Light;

                // Recargar UI
                LoadSettings();

                await DisplayAlert("Éxito", "✅ Ajustes restaurados", "OK");
            }
        }

        private async void OnClearAllClicked(object? sender, EventArgs e)
        {
            var confirm = await DisplayAlert(
                "⚠️ Borrar todos los datos",
                "Esto eliminará todas las preferencias guardadas localmente.\n\n" +
                "Los datos en Firebase NO se eliminarán.\n\n" +
                "¿Estás seguro?",
                "Borrar todo",
                "Cancelar");

            if (confirm)
            {
                PreferencesService.ClearAll();
                
                // Aplicar tema claro
                Application.Current!.UserAppTheme = AppTheme.Light;

                // Recargar UI
                LoadSettings();

                await DisplayAlert("Éxito", "✅ Todos los datos locales han sido eliminados", "OK");
            }
        }

        #endregion

        #region Helpers

        private async void ShowToast(string message)
        {
            // Mostrar mensaje temporal
            await DisplayAlert("", message, "OK");
        }

        #endregion
    }
}

