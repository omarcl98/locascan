using System.Collections.ObjectModel;
using apitest.Models;
using apitest.Services;

namespace apitest;

public partial class LocationsPage : ContentPage
{
    private readonly FirebaseDatabaseService _databaseService;
    private ObservableCollection<StorageLocation> _locations;
    private StorageLocation? _editingLocation;
    private string _selectedIcon = "📍";
    private double _currentLatitude;
    private double _currentLongitude;

    public ObservableCollection<StorageLocation> Locations
    {
        get => _locations;
        set
        {
            _locations = value;
            OnPropertyChanged();
        }
    }

    public LocationsPage()
    {
        InitializeComponent();
        _databaseService = new FirebaseDatabaseService();
        _locations = new ObservableCollection<StorageLocation>();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadLocationsAsync();
    }

    private async Task LoadLocationsAsync()
    {
        try
        {
            var locations = await _databaseService.GetAllLocationsAsync();
            
            foreach (var location in locations)
            {
                location.ProductCount = await _databaseService.GetProductCountByLocationAsync(location.Id!);
            }

            Locations.Clear();
            foreach (var location in locations)
            {
                Locations.Add(location);
            }

            TotalLocationsLabel.Text = Locations.Count.ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar ubicaciones: {ex.Message}", "OK");
        }
    }

    private async void OnGetCoordinatesClicked(object? sender, EventArgs e)
    {
        try
        {
            GetCoordsButton.IsEnabled = false;
            GetCoordsButton.Text = "...";

            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.High,
                Timeout = TimeSpan.FromSeconds(15)
            });

            if (location != null)
            {
                _currentLatitude = location.Latitude;
                _currentLongitude = location.Longitude;
                CoordinatesLabel.Text = $"{_currentLatitude:N4}, {_currentLongitude:N4}";
                
                try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(50)); } catch { }
            }
            else
            {
                await DisplayAlert("Error", "No se pudo obtener la ubicacion GPS", "OK");
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Error", "GPS no soportado en este dispositivo", "OK");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Permiso requerido", "Se necesita permiso de ubicacion", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error GPS: {ex.Message}", "OK");
        }
        finally
        {
            GetCoordsButton.IsEnabled = true;
            GetCoordsButton.Text = "GPS";
        }
    }

    private void OnIconSelected(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            Icon1.BackgroundColor = Color.FromArgb("#E0E0E0");
            Icon2.BackgroundColor = Color.FromArgb("#E0E0E0");
            Icon3.BackgroundColor = Color.FromArgb("#E0E0E0");
            Icon4.BackgroundColor = Color.FromArgb("#E0E0E0");
            Icon5.BackgroundColor = Color.FromArgb("#E0E0E0");

            button.BackgroundColor = Color.FromArgb("#6750A4");
            _selectedIcon = button.Text;
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var name = LocationNameEntry.Text?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlert("Error", "El nombre de la ubicacion es requerido", "OK");
            return;
        }

        try
        {
            SaveButton.IsEnabled = false;
            SaveButton.Text = "Guardando...";

            if (_editingLocation != null)
            {
                _editingLocation.Name = name;
                _editingLocation.Description = DescriptionEntry.Text?.Trim();
                _editingLocation.Address = AddressEntry.Text?.Trim();
                _editingLocation.IconEmoji = _selectedIcon;
                
                if (_currentLatitude != 0 || _currentLongitude != 0)
                {
                    _editingLocation.Latitude = _currentLatitude;
                    _editingLocation.Longitude = _currentLongitude;
                }

                var success = await _databaseService.UpdateLocationAsync(_editingLocation);
                if (success)
                {
                    await DisplayAlert("Exito", "Ubicacion actualizada", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo actualizar la ubicacion", "OK");
                }
            }
            else
            {
                var location = new StorageLocation
                {
                    Name = name,
                    Description = DescriptionEntry.Text?.Trim(),
                    Address = AddressEntry.Text?.Trim(),
                    Latitude = _currentLatitude,
                    Longitude = _currentLongitude,
                    IconEmoji = _selectedIcon
                };

                var id = await _databaseService.AddLocationAsync(location);
                if (!string.IsNullOrEmpty(id))
                {
                    await DisplayAlert("Exito", "Ubicacion guardada", "OK");
                }
            }

            ClearForm();
            await LoadLocationsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al guardar: {ex.Message}", "OK");
        }
        finally
        {
            SaveButton.IsEnabled = true;
            SaveButton.Text = "GUARDAR UBICACION";
        }
    }

    private void ClearForm()
    {
        LocationNameEntry.Text = string.Empty;
        DescriptionEntry.Text = string.Empty;
        AddressEntry.Text = string.Empty;
        CoordinatesLabel.Text = "No establecidas";
        _currentLatitude = 0;
        _currentLongitude = 0;
        _selectedIcon = "📍";
        _editingLocation = null;

        Icon1.BackgroundColor = Color.FromArgb("#E0E0E0");
        Icon2.BackgroundColor = Color.FromArgb("#E0E0E0");
        Icon3.BackgroundColor = Color.FromArgb("#E0E0E0");
        Icon4.BackgroundColor = Color.FromArgb("#E0E0E0");
        Icon5.BackgroundColor = Color.FromArgb("#E0E0E0");

        SaveButton.Text = "GUARDAR UBICACION";
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadLocationsAsync();
        LocationsRefreshView.IsRefreshing = false;
    }

    private void OnEditSwipe(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is StorageLocation location)
        {
            _editingLocation = location;
            LocationNameEntry.Text = location.Name;
            DescriptionEntry.Text = location.Description;
            AddressEntry.Text = location.Address;
            _selectedIcon = location.IconEmoji ?? "📍";
            _currentLatitude = location.Latitude;
            _currentLongitude = location.Longitude;

            if (location.Latitude != 0 || location.Longitude != 0)
            {
                CoordinatesLabel.Text = $"{location.Latitude:N4}, {location.Longitude:N4}";
            }

            SaveButton.Text = "ACTUALIZAR UBICACION";
        }
    }

    private async void OnDeleteSwipe(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is StorageLocation location)
        {
            var confirm = await DisplayAlert(
                "Confirmar eliminacion",
                $"Eliminar la ubicacion '{location.Name}'?\n\nLos productos asociados perderan la referencia a esta ubicacion.",
                "Eliminar",
                "Cancelar");

            if (confirm)
            {
                try
                {
                    var success = await _databaseService.DeleteLocationAsync(location.Id!);
                    if (success)
                    {
                        await DisplayAlert("Exito", "Ubicacion eliminada", "OK");
                        await LoadLocationsAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo eliminar la ubicacion", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Error al eliminar: {ex.Message}", "OK");
                }
            }
        }
    }
}

public class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        return !string.IsNullOrEmpty(value as string);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

