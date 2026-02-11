using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stayvelle.Models
{
    public class ServiceModel : CommonModel
    {
        [Key]
        public int ServiceId { get; set; }
        public string ServiceCategory { get; set; }
        public string SubCategory { get; set; }
        public string ServiceName { get; set; } 
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public bool IsComplementary { get; set; }
        public bool IsActive { get; set; }

        [NotMapped]
        public List<DocumentModel> Documents { get; set; } = new();

    }



}
