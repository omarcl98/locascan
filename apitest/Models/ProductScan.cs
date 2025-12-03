namespace apitest.Models
{
    public class ProductScan
    {
        public string? Id { get; set; }
        public string? Barcode { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Accuracy { get; set; }
        public DateTime ScanDate { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        
        // Referencia a la ubicación/almacén
        public string? LocationId { get; set; }
        public string? LocationName { get; set; }

        public ProductScan()
        {
            ScanDate = DateTime.UtcNow;
            Quantity = 1;
        }

        public string CoordinatesDisplay => $"{Latitude:N6}, {Longitude:N6}";
        public string DateDisplay => ScanDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        public string LocationDisplay => !string.IsNullOrEmpty(LocationName) ? LocationName : "Sin ubicación";
    }
}

