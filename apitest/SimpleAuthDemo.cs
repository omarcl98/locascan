using Microsoft.Maui.Controls;

namespace apitest
{
    public partial class SimpleAuthDemo : ContentPage
    {
        private bool _isLoggedIn = false;
        private string _userEmail = "";

        public SimpleAuthDemo()
        {
            InitializeComponent();
            CheckUserStatus();
        }

        private void CheckUserStatus()
        {
            if (_isLoggedIn)
            {
                UpdateUIForLoggedInUser();
            }
            else
            {
                UpdateUIForLoggedOutUser();
            }
        }

        private async void OnLoginClicked(object? sender, EventArgs e)
        {
            try
            {
                LoginButton.IsEnabled = false;
                StatusLabel.Text = "Conectando a Firebase...";
                StatusLabel.TextColor = Colors.Blue;

                // Simular proceso de autenticación con Google
                await Task.Delay(2000);

                // Simular éxito de autenticación
                _isLoggedIn = true;
                _userEmail = "usuario.demo@gmail.com";
                UpdateUIForLoggedInUser();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error de autenticación: {ex.Message}", "OK");
                UpdateUIForLoggedOutUser();
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }

        private void OnLogoutClicked(object? sender, EventArgs e)
        {
            _isLoggedIn = false;
            _userEmail = "";
            UpdateUIForLoggedOutUser();
        }

        private void UpdateUIForLoggedInUser()
        {
            StatusLabel.Text = "✓ Autenticado con Firebase";
            StatusLabel.TextColor = Colors.Green;
            
            UserInfoLabel.Text = $"Usuario: {_userEmail}";
            UserInfoLabel.IsVisible = true;

            LoginButton.IsVisible = false;
            LogoutButton.IsVisible = true;
        }

        private void UpdateUIForLoggedOutUser()
        {
            StatusLabel.Text = "No autenticado";
            StatusLabel.TextColor = Colors.Gray;
            
            UserInfoLabel.IsVisible = false;

            LoginButton.IsVisible = true;
            LogoutButton.IsVisible = false;
        }
    }
}
