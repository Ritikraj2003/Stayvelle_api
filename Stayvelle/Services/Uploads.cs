using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stayvelle.Services
{
    public static class Uploads
    {
        public static async Task<string> UploadImage(string newfileName, IFormFile file, string subDirectory, string baseUrl)
        {
            if (file == null || file.Length == 0)
                return null;
            
            string uploadsFolder = "Uploads";
            string uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), uploadsFolder, subDirectory);
            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }
            
            string extension = Path.GetExtension(file.FileName);
            string finalFileName = $"{newfileName}_{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadsDirectory, finalFileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            // Return full URL
            // Ensure forward slashes for URL
            string relativePath = $"{uploadsFolder}/{subDirectory}/{finalFileName}";
            return $"{baseUrl.TrimEnd('/')}/{relativePath}";
        }



        //public static async Task<string> UploadImage(string newfileName, IFormFile file, string subDirectory)
        //{
        //    if (file == null || file.Length == 0)
        //        return null;

        //    string uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", subDirectory);
        //    if (!Directory.Exists(uploadsDirectory))
        //    {
        //        Directory.CreateDirectory(uploadsDirectory);
        //    }

        //    string extension = Path.GetExtension(file.FileName);
        //    string finalFileName = $"{newfileName}_{Guid.NewGuid()}{extension}";
        //    string filePath = Path.Combine(uploadsDirectory, finalFileName);

        //    using (var stream = new FileStream(filePath, FileMode.Create))
        //    {
        //        await file.CopyToAsync(stream);
        //    }

        //    return filePath;
        //}
    }
}
