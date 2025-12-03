using Plugin.Firebase.Auth;

namespace apitest
{
    public partial class MainPage : ContentPage
    {
        private bool _isLoggedIn = false;
        private string _userEmail = "";
        private string _userName = "";
        private IFirebaseAuth _firebaseAuth;

        public MainPage()
        {
            InitializeComponent();
            _firebaseAuth = CrossFirebaseAuth.Current;
            CheckUserStatus();
        }

        private void CheckUserStatus()
        {
            try
            {
                // Verificar si hay un usuario autenticado con Firebase
                var currentUser = _firebaseAuth.CurrentUser;
                
                if (currentUser != null)
                {
                    _isLoggedIn = true;
                    _userEmail = currentUser.Email ?? "";
                    _userName = currentUser.DisplayName ?? currentUser.Email?.Split('@')[0] ?? "";
                    UpdateUIForLoggedInUser();
                }
                else
                {
                    UpdateUIForLoggedOutUser();
                }
            }
            catch
            {
                UpdateUIForLoggedOutUser();
            }
        }

        private async void OnLoginClicked(object? sender, EventArgs e)
        {
            try
            {
                // Validar campos de entrada
                if (string.IsNullOrWhiteSpace(EmailEntry.Text))
                {
                    await DisplayAlert("Error", "Por favor ingresa tu email", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
                {
                    await DisplayAlert("Error", "Por favor ingresa tu contraseña", "OK");
                    return;
                }

                LoginButton.IsEnabled = false;
                StatusLabel.Text = "Conectando con Firebase...";
                StatusLabel.TextColor = Application.Current?.Resources["Primary"] as Color ?? Colors.Blue;

                // Autenticación real con Firebase
                var result = await _firebaseAuth.SignInWithEmailAndPasswordAsync(EmailEntry.Text.Trim(), PasswordEntry.Text);
                
                if (result != null)
                {
                    _isLoggedIn = true;
                    _userEmail = result.Email ?? "";
                    _userName = result.DisplayName ?? _userEmail.Split('@')[0];
                    
                    await DisplayAlert("¡Éxito!", 
                        $"Conexión exitosa con Firebase!\n" +
                        $"Usuario: {_userName}\n" +
                        $"Email: {_userEmail}\n" +
                        $"UID: {result.Uid}\n\n" +
                        $"✅ Firebase Authentication funcionando correctamente", "OK");
                    
                    // Navegar a la página de inventario después de la autenticación exitosa
                    await Shell.Current.GoToAsync("//InventoryPage");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo autenticar con Firebase", "OK");
                    UpdateUIForLoggedOutUser();
                }
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("Error de Firebase", 
                    $"Error al conectar con Firebase: {ex.Message}\n\n" +
                    $"Verifica que:\n" +
                    $"• Firebase esté configurado\n" +
                    $"• El email y contraseña sean correctos\n" +
                    $"• Tengas conexión a internet", "OK");
                UpdateUIForLoggedOutUser();
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }

        private async void OnLogoutClicked(object? sender, EventArgs e)
        {
            try
            {
                // Cerrar sesión real con Firebase
                await _firebaseAuth.SignOutAsync();
                
                _isLoggedIn = false;
                _userEmail = "";
                _userName = "";
                
                // Limpiar campos de entrada
                EmailEntry.Text = "";
                PasswordEntry.Text = "";
                
                UpdateUIForLoggedOutUser();
                
                await DisplayAlert("Sesión cerrada", 
                    "Has cerrado sesión exitosamente de Firebase\n\n" +
                    "✅ Conexión con Firebase terminada correctamente", "OK");
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("Error", $"Error al cerrar sesión: {ex.Message}", "OK");
            }
        }

        private void UpdateUIForLoggedInUser()
        {
            StatusLabel.Text = "✓ Conectado a Firebase Authentication";
            StatusLabel.TextColor = Application.Current?.Resources["Primary"] as Color ?? Colors.Green;
            
            UserInfoLabel.Text = $"Usuario: {_userName}\nEmail: {_userEmail}";
            UserInfoLabel.IsVisible = true;

            // Ocultar formulario de login
            LoginFrame.IsVisible = false;
            LogoutButton.IsVisible = true;
        }

        private void UpdateUIForLoggedOutUser()
        {
            StatusLabel.Text = "No has iniciado sesión";
            StatusLabel.TextColor = Application.Current?.Resources["OnSurfaceVariant"] as Color ?? Colors.Gray;
            
            UserInfoLabel.IsVisible = false;

            // Mostrar formulario de login
            LoginFrame.IsVisible = true;
            LogoutButton.IsVisible = false;
        }
    }
}
