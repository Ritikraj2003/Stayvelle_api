using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stayvelle.Models
{
    public class RoomDiscountModel
    {
        [Key]
        public int DiscountId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public RoomModel? Room { get; set; }

        public string DiscountType { get; set; } = "FLAT"; // PERCENTAGE / FLAT
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        
        public string CreatedBy { get; set; } = "system";
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
