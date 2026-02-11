using Microsoft.AspNetCore.Http; // Required for IFormFile

namespace Stayvelle.Models.DTOs
{
    public class CreateServiceDto
    {
        public string ServiceCategory { get; set; } = string.Empty;
        public string SubCategory { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsComplementary { get; set; }
        public bool IsActive { get; set; } = true;
        public IFormFile? Image { get; set; } // For image upload
    }

    public class UpdateServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceCategory { get; set; } = string.Empty;
        public string SubCategory { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsComplementary { get; set; }
        public bool IsActive { get; set; }
        public IFormFile? Image { get; set; } // For updating image
    }

    public class ServiceResponseDto : ServiceModel
    {
        // Additional property for the image path/url if needed
        // Assuming ServiceModel has basic properties.
        // We'll return the document data separately or attach it here.
        // The user wants to store it in DocumentModel.
        // When fetching all services, we might want the image path.
        public string? ImagePath { get; set; }
    }
}
