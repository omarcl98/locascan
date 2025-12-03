namespace apitest.Models
{
    public class StorageLocation
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? IconEmoji { get; set; }
        public string? Color { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UserId { get; set; }
        public int ProductCount { get; set; }

        public StorageLocation()
        {
            CreatedAt = DateTime.UtcNow;
            IconEmoji = "📍";
            Color = "#6750A4";
            ProductCount = 0;
        }

        public string CoordinatesDisplay => Latitude != 0 || Longitude != 0 
            ? $"{Latitude:N4}, {Longitude:N4}" 
            : "Sin coordenadas";
            
        public string DateDisplay => CreatedAt.ToLocalTime().ToString("dd/MM/yyyy");
    }
}

