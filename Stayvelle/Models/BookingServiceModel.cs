using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stayvelle.Models
{
    public class BookingServiceModel
    {
        [Key]
        public int BookingServiceId { get; set; }

        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public BookingModel? Booking { get; set; }

        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public ServiceModel? Service { get; set; }

        public string ServiceCategory { get; set; }
        public string SubCategory { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public bool IsComplementary { get; set; }
        public int Quantity { get; set; }
        public DateTime ServiceDate { get; set; }
        public string ServiceStatus { get; set; } = "Requested"; 
    }
}
