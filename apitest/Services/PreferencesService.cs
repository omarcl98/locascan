namespace apitest.Services
{
    /// <summary>
    /// Servicio para manejar las preferencias locales de la aplicación
    /// </summary>
    public static class PreferencesService
    {
        // Claves de preferencias
        private const string KEY_DARK_MODE = "dark_mode";
        private const string KEY_AUTO_SCAN = "auto_scan";
        private const string KEY_VIBRATE_ON_SCAN = "vibrate_on_scan";
        private const string KEY_SOUND_ON_SCAN = "sound_on_scan";
        private const string KEY_LAST_LATITUDE = "last_latitude";
        private const string KEY_LAST_LONGITUDE = "last_longitude";
        private const string KEY_USER_EMAIL = "user_email";
        private const string KEY_USER_NAME = "user_name";
        private const string KEY_FIRST_LAUNCH = "first_launch";
        private const string KEY_SCAN_COUNT = "scan_count";
        private const string KEY_LAST_SYNC_DATE = "last_sync_date";
        private const string KEY_DEFAULT_QUANTITY = "default_quantity";
        private const string KEY_CAMERA_FLASH = "camera_flash";

        #region Configuración de Tema

        /// <summary>
        /// Obtiene o establece si el modo oscuro está activado
        /// </summary>
        public static bool IsDarkMode
        {
            get => Preferences.Get(KEY_DARK_MODE, false);
            set => Preferences.Set(KEY_DARK_MODE, value);
        }

        #endregion

        #region Configuración de Escaneo

        /// <summary>
        /// Obtiene o establece si el escaneo automático está activado
        /// </summary>
        public static bool AutoScanEnabled
        {
            get => Preferences.Get(KEY_AUTO_SCAN, true);
            set => Preferences.Set(KEY_AUTO_SCAN, value);
        }

        /// <summary>
        /// Obtiene o establece si vibrar al escanear está activado
        /// </summary>
        public static bool VibrateOnScan
        {
            get => Preferences.Get(KEY_VIBRATE_ON_SCAN, true);
            set => Preferences.Set(KEY_VIBRATE_ON_SCAN, value);
        }

        /// <summary>
        /// Obtiene o establece si el sonido al escanear está activado
        /// </summary>
        public static bool SoundOnScan
        {
            get => Preferences.Get(KEY_SOUND_ON_SCAN, true);
            set => Preferences.Set(KEY_SOUND_ON_SCAN, value);
        }

        /// <summary>
        /// Obtiene o establece si el flash de la cámara está activado
        /// </summary>
        public static bool CameraFlashEnabled
        {
            get => Preferences.Get(KEY_CAMERA_FLASH, false);
            set => Preferences.Set(KEY_CAMERA_FLASH, value);
        }

        /// <summary>
        /// Obtiene o establece la cantidad por defecto para nuevos productos
        /// </summary>
        public static int DefaultQuantity
        {
            get => Preferences.Get(KEY_DEFAULT_QUANTITY, 1);
            set => Preferences.Set(KEY_DEFAULT_QUANTITY, value);
        }

        #endregion

        #region Última Ubicación

        /// <summary>
        /// Obtiene o establece la última latitud conocida
        /// </summary>
        public static double LastLatitude
        {
            get => Preferences.Get(KEY_LAST_LATITUDE, 0.0);
            set => Preferences.Set(KEY_LAST_LATITUDE, value);
        }

        /// <summary>
        /// Obtiene o establece la última longitud conocida
        /// </summary>
        public static double LastLongitude
        {
            get => Preferences.Get(KEY_LAST_LONGITUDE, 0.0);
            set => Preferences.Set(KEY_LAST_LONGITUDE, value);
        }

        /// <summary>
        /// Guarda la última ubicación conocida
        /// </summary>
        public static void SaveLastLocation(double latitude, double longitude)
        {
            LastLatitude = latitude;
            LastLongitude = longitude;
        }

        /// <summary>
        /// Obtiene la última ubicación como tupla
        /// </summary>
        public static (double Latitude, double Longitude) GetLastLocation()
        {
            return (LastLatitude, LastLongitude);
        }

        /// <summary>
        /// Verifica si hay una ubicación guardada
        /// </summary>
        public static bool HasSavedLocation => LastLatitude != 0.0 || LastLongitude != 0.0;

        #endregion

        #region Información del Usuario

        /// <summary>
        /// Obtiene o establece el email del usuario
        /// </summary>
        public static string UserEmail
        {
            get => Preferences.Get(KEY_USER_EMAIL, string.Empty);
            set => Preferences.Set(KEY_USER_EMAIL, value);
        }

        /// <summary>
        /// Obtiene o establece el nombre del usuario
        /// </summary>
        public static string UserName
        {
            get => Preferences.Get(KEY_USER_NAME, string.Empty);
            set => Preferences.Set(KEY_USER_NAME, value);
        }

        /// <summary>
        /// Guarda la información del usuario
        /// </summary>
        public static void SaveUserInfo(string email, string name)
        {
            UserEmail = email;
            UserName = name;
        }

        /// <summary>
        /// Limpia la información del usuario (al cerrar sesión)
        /// </summary>
        public static void ClearUserInfo()
        {
            UserEmail = string.Empty;
            UserName = string.Empty;
        }

        #endregion

        #region Estadísticas y Contadores

        /// <summary>
        /// Obtiene o establece si es el primer lanzamiento de la app
        /// </summary>
        public static bool IsFirstLaunch
        {
            get => Preferences.Get(KEY_FIRST_LAUNCH, true);
            set => Preferences.Set(KEY_FIRST_LAUNCH, value);
        }

        /// <summary>
        /// Obtiene o establece el contador de escaneos realizados
        /// </summary>
        public static int ScanCount
        {
            get => Preferences.Get(KEY_SCAN_COUNT, 0);
            set => Preferences.Set(KEY_SCAN_COUNT, value);
        }

        /// <summary>
        /// Incrementa el contador de escaneos
        /// </summary>
        public static void IncrementScanCount()
        {
            ScanCount++;
        }

        /// <summary>
        /// Obtiene o establece la fecha de la última sincronización
        /// </summary>
        public static DateTime LastSyncDate
        {
            get
            {
                var ticks = Preferences.Get(KEY_LAST_SYNC_DATE, 0L);
                return ticks == 0 ? DateTime.MinValue : new DateTime(ticks);
            }
            set => Preferences.Set(KEY_LAST_SYNC_DATE, value.Ticks);
        }

        /// <summary>
        /// Actualiza la fecha de última sincronización a ahora
        /// </summary>
        public static void UpdateLastSyncDate()
        {
            LastSyncDate = DateTime.Now;
        }

        #endregion

        #region Utilidades

        /// <summary>
        /// Limpia todas las preferencias (reset completo)
        /// </summary>
        public static void ClearAll()
        {
            Preferences.Clear();
        }

        /// <summary>
        /// Limpia una preferencia específica
        /// </summary>
        public static void Remove(string key)
        {
            Preferences.Remove(key);
        }

        /// <summary>
        /// Verifica si existe una preferencia
        /// </summary>
        public static bool ContainsKey(string key)
        {
            return Preferences.ContainsKey(key);
        }

        #endregion
    }
}


