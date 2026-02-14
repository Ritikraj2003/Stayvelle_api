namespace Stayvelle.Models.DTOs
{
    public class BookingDetailDto
    {
        public int BookingId { get; set; }
        public string Status { get; set; }
        public RoomDto Room { get; set; }
        public List<GuestDto> Guests { get; set; } = new();
        public List<ServiceDto> Services { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    public class RoomDto
    {
        public string RoomNumber { get; set; }
        public decimal Price { get; set; }
    }

    public class GuestDto
    {
        public int GuestId { get; set; }
        public string Name { get; set; }
        public bool Primary { get; set; }
    }

    public class ServiceDto
    {
        public string Name { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
    }

    public class AddBookingServiceDto
    {
        public int BookingId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCategory { get; set; }
        public string SubCategory { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public DateTime ServiceDate { get; set; }
        public string? ServiceStatus { get; set; }
    }
}
