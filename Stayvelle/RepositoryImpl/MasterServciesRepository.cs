using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Services;
using Microsoft.Extensions.Configuration;

namespace Stayvelle.RepositoryImpl
{
    public class MasterServciesRepository : IMasterServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public MasterServciesRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Response<List<ServiceResponseDto>>> GetAllServicesAsync()
        {
            var response = new Response<List<ServiceResponseDto>>();
            try
            {
                var services = await _context.ServiceModel.ToListAsync();
                
                // Fetch images for all services
                var serviceIds = services.Select(s => s.ServiceId).ToList();
                var imageDocs = await _context.DocumentModel
                    .Where(d => d.EntityType == "SERVICE" && serviceIds.Contains(d.EntityId))
                    .ToListAsync();

                var dtos = services.Select(s => new ServiceResponseDto
                {
                    ServiceId = s.ServiceId,
                    ServiceCategory = s.ServiceCategory,
                    SubCategory=s.SubCategory,
                    ServiceName = s.ServiceName,
                    Price = s.Price,
                    Unit = s.Unit,
                    IsComplementary = s.IsComplementary,
                    IsActive = s.IsActive,
                    CreatedBy = s.CreatedBy,
                    CreatedOn = s.CreatedOn,
                    ModifiedBy = s.ModifiedBy,
                    ModifiedOn = s.ModifiedOn,
                    Documents = imageDocs
                        .Where(d => d.EntityId == s.ServiceId)
                        .Select(d => new DocumentDto
                        {
                            DocumentId = d.DocumentId,
                            EntityType = d.EntityType,
                            EntityId = d.EntityId,
                            DocumentType = d.DocumentType,
                            FileName = d.FileName,
                            FilePath = d.FilePath,
                            IsPrimary = d.IsPrimary,
                            Description = d.Description
                        }).ToList()
                }).ToList();

                response.Success = true;
                response.Data = dtos;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<Response<ServiceResponseDto?>> GetServiceByIdAsync(int serviceId)
        {
            var response = new Response<ServiceResponseDto?>();
            try
            {
                var service = await _context.ServiceModel.FindAsync(serviceId);
                if (service == null)
                {
                    response.Success = false;
                    response.Message = "Service not found";
                    return response;
                }

                var docs = await _context.DocumentModel
                    .Where(d => d.EntityType == "SERVICE" && d.EntityId == serviceId)
                    .ToListAsync();

                var dto = new ServiceResponseDto
                {
                    ServiceId = service.ServiceId,
                    ServiceCategory = service.ServiceCategory,
                    SubCategory = service.SubCategory,
                    ServiceName = service.ServiceName,
                    Price = service.Price,
                    Unit = service.Unit,
                    IsComplementary = service.IsComplementary,
                    IsActive = service.IsActive,
                    CreatedBy = service.CreatedBy,
                    CreatedOn = service.CreatedOn,
                    ModifiedBy = service.ModifiedBy,
                    ModifiedOn = service.ModifiedOn,
                    Documents = docs.Select(d => new DocumentDto
                    {
                        DocumentId = d.DocumentId,
                        EntityType = d.EntityType,
                        EntityId = d.EntityId,
                        DocumentType = d.DocumentType,
                        FileName = d.FileName,
                        FilePath = d.FilePath,
                        IsPrimary = d.IsPrimary,
                        Description = d.Description
                    }).ToList()
                };

                response.Success = true;
                response.Data = dto;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<Response<ServiceModel>> CreateServiceAsync(CreateServiceDto dto)
        {
            var response = new Response<ServiceModel>();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var service = new ServiceModel
                {
                    ServiceCategory = dto.ServiceCategory,
                    ServiceName = dto.ServiceName,
                    Price = dto.Price,
                    SubCategory=dto.SubCategory,
                    Unit = dto.Unit,
                    IsComplementary = dto.IsComplementary,
                    IsActive = dto.IsActive,
                    CreatedBy = "system",
                    CreatedOn = DateTime.UtcNow
                };

                _context.ServiceModel.Add(service);
                await _context.SaveChangesAsync();

                if (dto.Documents != null && dto.Documents.Any())
                {
                    string baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7252";
                    var newDocs = new List<DocumentModel>();
                    foreach (var docDto in dto.Documents)
                    {
                        if (docDto.File != null)
                        {
                            string filePath = await Uploads.UploadImage(dto.ServiceName, docDto.File, "SERVICE", baseUrl);
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                var doc = new DocumentModel
                                {
                                    EntityType = "SERVICE",
                                    EntityId = service.ServiceId,
                                    DocumentType = !string.IsNullOrEmpty(docDto.DocumentType) ? docDto.DocumentType : "ServiceImage",
                                    FileName = !string.IsNullOrEmpty(docDto.FileName) ? docDto.FileName : docDto.File.FileName,
                                    FilePath = filePath,
                                    Description = docDto.Description,
                                    IsPrimary = docDto.IsPrimary,
                                    CreatedBy = "system",
                                    CreatedOn = DateTime.UtcNow
                                };
                                newDocs.Add(doc);
                            }
                        }
                    }

                    if (newDocs.Any())
                    {
                        _context.DocumentModel.AddRange(newDocs);
                        await _context.SaveChangesAsync();

                        service.Documents = newDocs.Select(d => new DocumentDto
                        {
                            DocumentId = d.DocumentId,
                            EntityType = d.EntityType,
                            EntityId = d.EntityId,
                            DocumentType = d.DocumentType,
                            FileName = d.FileName,
                            FilePath = d.FilePath,
                            IsPrimary = d.IsPrimary,
                            Description = d.Description
                        }).ToList();
                    }
                }

                await transaction.CommitAsync();
                
                response.Success = true;
                response.Data = service;
                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<Response<ServiceModel>> UpdateServiceAsync(int serviceId, UpdateServiceDto dto)
        {
             var response = new Response<ServiceModel>();
             using var transaction = await _context.Database.BeginTransactionAsync();
             try
             {
                 var service = await _context.ServiceModel.FindAsync(serviceId);
                 if (service == null)
                 {
                     response.Success = false;
                     response.Message = "Service not found";
                     return response;
                 }
                 
                 service.ServiceCategory = dto.ServiceCategory;
                 service.ServiceName = dto.ServiceName;
                service.SubCategory = dto.SubCategory;
                 service.Price = dto.Price;
                 service.Unit = dto.Unit;
                 service.IsComplementary = dto.IsComplementary;
                 service.IsActive = dto.IsActive;
                 service.ModifiedBy = "system";
                 service.ModifiedOn = DateTime.UtcNow;
                 
                 _context.ServiceModel.Update(service);
                 await _context.SaveChangesAsync();

                 if (dto.Documents != null && dto.Documents.Any())
                 {
                     string baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7252";
                     
                     foreach(var docDto in dto.Documents)
                     {
                         if (docDto.File != null)
                         {
                             // Use ServiceName for folder, or maybe keep it "SERVICE"? Previous code used service.ServiceName
                             string filePath = await Uploads.UploadImage(service.ServiceName, docDto.File, "SERVICE", baseUrl);
                             if (!string.IsNullOrEmpty(filePath))
                             {
                                 var doc = new DocumentModel
                                 {
                                     EntityType = "SERVICE",
                                     EntityId = service.ServiceId,
                                     DocumentType = !string.IsNullOrEmpty(docDto.DocumentType) ? docDto.DocumentType : "ServiceImage",
                                     FileName = !string.IsNullOrEmpty(docDto.FileName) ? docDto.FileName : docDto.File.FileName,
                                     FilePath = filePath,
                                     Description = docDto.Description,
                                     IsPrimary = docDto.IsPrimary,
                                     CreatedBy = "system",
                                     CreatedOn = DateTime.UtcNow
                                 };
                                 _context.DocumentModel.Add(doc);
                             }
                         }
                     }
                     await _context.SaveChangesAsync();
                 }

                 await transaction.CommitAsync();
                 response.Success = true;
                 response.Data = service;
                 return response;
             }
             catch(Exception ex)
             {
                 await transaction.RollbackAsync();
                 response.Success = false;
                 response.Message = ex.Message;
                 return response;
             }
        }

        public async Task<Response<bool>> DeleteServiceAsync(int serviceId)
        {
            var response = new Response<bool>();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var service = await _context.ServiceModel.FindAsync(serviceId);
                if (service == null)
                {
                    response.Success = false;
                    response.Message = "Service not found";
                    return response;
                }

                // Delete associated documents
                var docs = await _context.DocumentModel
                    .Where(d => d.EntityType == "SERVICE" && d.EntityId == serviceId)
                    .ToListAsync();
                
                _context.DocumentModel.RemoveRange(docs);
                
                _context.ServiceModel.Remove(service);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Success = true;
                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
        }
    }
}
