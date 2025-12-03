using Plugin.Firebase.Auth;

namespace apitest
{
    public partial class SplashPage : ContentPage
    {
        private IFirebaseAuth _firebaseAuth;

        public SplashPage()
        {
            InitializeComponent();
            _firebaseAuth = CrossFirebaseAuth.Current;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Iniciar el timer de 3 segundos
            await Task.Delay(3000);
            
            // Verificar si hay una sesión activa después del delay
            await CheckUserStatusAndNavigate();
        }

        private async Task CheckUserStatusAndNavigate()
        {
            try
            {
                // Verificar si hay un usuario autenticado con Firebase
                var currentUser = _firebaseAuth.CurrentUser;
                
                if (currentUser != null)
                {
                    // Si ya hay un usuario autenticado, navegar directamente a InventoryPage
                    await Shell.Current.GoToAsync("//InventoryPage");
                }
                else
                {
                    // Si no hay usuario autenticado, navegar a MainPage (login)
                    await Shell.Current.GoToAsync("//MainPage");
                }
            }
            catch (Exception ex)
            {
                // En caso de error, navegar a MainPage por defecto
                await Shell.Current.GoToAsync("//MainPage");
            }
        }
    }
}


