using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Models;

using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Stayvelle.RepositoryImpl
{
    public class RoomRepository : IRoom
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public RoomRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Create
        public async Task<Response<RoomModel>> CreateRoomAsync(CreateRoomDTO createRoomDTO)
        {
            Response<RoomModel> response = new Response<RoomModel>();
            try
            {
                // Check if room number already exists
                var existingRoom = await _context.RoomModel
                    .FirstOrDefaultAsync(r => r.RoomNumber == createRoomDTO.RoomNumber);
                
                if (existingRoom != null)
                {
                    response.Success = false;
                    response.Message = "Room with this room number already exists";
                    response.Data = null;
                    return response;
                }

                // Convert Images list to JSON string if provided
                //string? imagesJson = null;
                //if (createRoomDTO.Images != null && createRoomDTO.Images.Count > 0)
                //{
                //    // Filter out any null or empty strings
                //    var validImages = createRoomDTO.Images.Where(img => !string.IsNullOrWhiteSpace(img)).ToList();
                //    if (validImages.Any())
                //    {
                //        imagesJson = JsonSerializer.Serialize(validImages);
                //    }
                //}

                var room = new RoomModel
                {
                    RoomNumber = createRoomDTO.RoomNumber,
                    RoomQrToken = Guid.NewGuid().ToString(),
                    Price = createRoomDTO.Price,
                    MaxOccupancy = createRoomDTO.MaxOccupancy,
                    Floor = createRoomDTO.Floor,
                    NumberOfBeds = createRoomDTO.NumberOfBeds,
                    ACType = createRoomDTO.AcType,
                    BathroomType = createRoomDTO.BathroomType,
                    RoomStatus = createRoomDTO.RoomStatus,
                    RoomType = createRoomDTO.RoomType,
                    IsActive = createRoomDTO.IsActive,
                    Description = createRoomDTO.Description,
                    IsTv = createRoomDTO.IsTv,
                   
                    CreatedBy = createRoomDTO.CreatedBy,
                    CreatedOn = createRoomDTO.CreatedOn,
                    ModifiedBy = createRoomDTO.ModifiedBy,
                    ModifiedOn = createRoomDTO.ModifiedOn
                };

                _context.RoomModel.Add(room);
                await _context.SaveChangesAsync();

                // Handle Documents
                if (createRoomDTO.Documents != null && createRoomDTO.Documents.Any())
                {
                    string baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7252";
                    foreach (var doc in createRoomDTO.Documents)
                    {
                        if (doc.File != null && doc.File.Length > 0)
                        {
                            string filePath = await Stayvelle.Services.Uploads.UploadImage(doc.FileName, doc.File, "RoomDocuments", baseUrl);
                            
                            var document = new DocumentModel
                            {
                                EntityType = "ROOM",
                                EntityId = room.Id,
                                DocumentType = doc.DocumentType ?? "ROOM_IMAGE",
                                FileName = doc.FileName,
                                Description = doc.Description,
                                FilePath = filePath,
                                IsPrimary = doc.IsPrimary,
                                CreatedBy = createRoomDTO.CreatedBy,
                                CreatedOn = DateTime.UtcNow,
                                ModifiedOn = DateTime.UtcNow
                            };
                            
                            _context.DocumentModel.Add(document);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // Reload room with documents if needed, or just assign to response properties if using DTOs
                // But RoomModel has [NotMapped] Documents List<DocumentDto>
                // We should populate it for the response

                var savedDocuments = await _context.DocumentModel
                    .Where(d => d.EntityType == "ROOM" && d.EntityId == room.Id)
                    .Select(d => new DocumentDto
                    {
                        DocumentId = d.DocumentId,
                        EntityType = d.EntityType,
                        EntityId = d.EntityId,
                        DocumentType = d.DocumentType,
                        FileName = d.FileName,
                        Description = d.Description,
                        FilePath = d.FilePath,
                        IsPrimary = d.IsPrimary
                    }).ToListAsync();

                room.Documents = savedDocuments;

                response.Success = true;
                response.Message = "Room created successfully";
                response.Data = room;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Read - Get All Rooms
        public async Task<Response<List<RoomModel>>> GetAllRoomsAsync()
        {
            var response = new Response<List<RoomModel>>();
            try
            {
                var rooms = await _context.RoomModel
                    .OrderBy(r => r.RoomNumber)
                    .ToListAsync();
                
                // Populate Documents
                if (rooms.Any())
                {
                    var roomIds = rooms.Select(r => r.Id).ToList();
                    var documents = await _context.DocumentModel
                        .Where(d => d.EntityType == "ROOM" && roomIds.Contains(d.EntityId))
                        .ToListAsync();

                    foreach (var room in rooms)
                    {
                        room.Documents = documents
                            .Where(d => d.EntityId == room.Id)
                            .Select(d => new DocumentDto
                            {
                                DocumentId = d.DocumentId,
                                EntityType = d.EntityType,
                                EntityId = d.EntityId,
                                DocumentType = d.DocumentType,
                                FileName = d.FileName,
                                Description = d.Description,
                                FilePath = d.FilePath,
                                IsPrimary = d.IsPrimary
                            }).ToList();
                    }
                }

                response.Success = true;
                response.Message = "success";
                response.Data = rooms;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Read - Get Room By Id
        public async Task<Response<RoomModel?>> GetRoomByIdAsync(int id)
        {
            var response = new Response<RoomModel?>();
            try
            {
                var room = await _context.RoomModel.FindAsync(id);

                if (room == null)
                {
                    response.Success = false;
                    response.Message = "Room not found";
                    response.Data = null;
                    return response;
                }

                // Populate Documents
                var documents = await _context.DocumentModel
                    .Where(d => d.EntityType == "ROOM" && d.EntityId == room.Id)
                    .ToListAsync();

                room.Documents = documents.Select(d => new DocumentDto
                {
                    DocumentId = d.DocumentId,
                    EntityType = d.EntityType,
                    EntityId = d.EntityId,
                    DocumentType = d.DocumentType,
                    FileName = d.FileName,
                    Description = d.Description,
                    FilePath = d.FilePath,
                    IsPrimary = d.IsPrimary
                }).ToList();

                response.Success = true;
                response.Message = "Success";
                response.Data = room;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Read - Get Room By Room Number
        public async Task<Response<RoomModel?>> GetRoomByRoomNumberAsync(string roomNumber)
        {
            var response = new Response<RoomModel?>();
            try
            {
                var room = await _context.RoomModel
                    .FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);

                if (room != null)
                {
                    // Populate Documents
                    var documents = await _context.DocumentModel
                        .Where(d => d.EntityType == "ROOM" && d.EntityId == room.Id)
                        .ToListAsync();

                    room.Documents = documents.Select(d => new DocumentDto
                    {
                        DocumentId = d.DocumentId,
                        EntityType = d.EntityType,
                        EntityId = d.EntityId,
                        DocumentType = d.DocumentType,
                        FileName = d.FileName,
                        Description = d.Description,
                        FilePath = d.FilePath,
                        IsPrimary = d.IsPrimary
                    }).ToList();
                }

                response.Success = room != null;
                response.Message = room != null ? "Success" : "Room not found";
                response.Data = room;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Read - Get Rooms By Status
        public async Task<Response<List<RoomModel>>> GetRoomsByStatusAsync(string status)
        {
            var response = new Response<List<RoomModel>>();
            try
            {
                var rooms = await _context.RoomModel
                    .Where(r => r.RoomStatus == status)
                    .OrderBy(r => r.RoomNumber)
                    .ToListAsync();

                // Populate Documents
                if (rooms.Any())
                {
                    var roomIds = rooms.Select(r => r.Id).ToList();
                    var documents = await _context.DocumentModel
                        .Where(d => d.EntityType == "ROOM" && roomIds.Contains(d.EntityId))
                        .ToListAsync();

                    foreach (var room in rooms)
                    {
                        room.Documents = documents
                            .Where(d => d.EntityId == room.Id)
                            .Select(d => new DocumentDto
                            {
                                DocumentId = d.DocumentId,
                                EntityType = d.EntityType,
                                EntityId = d.EntityId,
                                DocumentType = d.DocumentType,
                                FileName = d.FileName,
                                Description = d.Description,
                                FilePath = d.FilePath,
                                IsPrimary = d.IsPrimary
                            }).ToList();
                    }
                }

                response.Success = true;
                response.Message = "Success";
                response.Data = rooms;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Read - Get Rooms By Type
        public async Task<Response<List<RoomModel>>> GetRoomsByTypeAsync(string roomType)
        {
            var response = new Response<List<RoomModel>>();
            try
            {
                var rooms = await _context.RoomModel
                    .Where(r => r.RoomType == roomType)
                    .OrderBy(r => r.RoomNumber)
                    .ToListAsync();

                // Populate Documents
                if (rooms.Any())
                {
                    var roomIds = rooms.Select(r => r.Id).ToList();
                    var documents = await _context.DocumentModel
                        .Where(d => d.EntityType == "ROOM" && roomIds.Contains(d.EntityId))
                        .ToListAsync();

                    foreach (var room in rooms)
                    {
                        room.Documents = documents
                            .Where(d => d.EntityId == room.Id)
                            .Select(d => new DocumentDto
                            {
                                DocumentId = d.DocumentId,
                                EntityType = d.EntityType,
                                EntityId = d.EntityId,
                                DocumentType = d.DocumentType,
                                FileName = d.FileName,
                                Description = d.Description,
                                FilePath = d.FilePath,
                                IsPrimary = d.IsPrimary
                            }).ToList();
                    }
                }

                response.Success = true;
                response.Message = "Success";
                response.Data = rooms;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Update
        public async Task<Response<RoomModel?>> UpdateRoomAsync(int id, UpdateRoomDTO updateRoomDTO)
        {
            var response = new Response<RoomModel?>();
            try
            {
                var existingRoomResponse = await GetRoomByIdAsync(id);

                if (!existingRoomResponse.Success || existingRoomResponse.Data == null)
                {
                    response.Success = false;
                    response.Message = "Room not found";
                    response.Data = null;
                    return response;
                }

                var existingRoom = existingRoomResponse.Data;

                // Check if room number is being changed and already exists
                if (!string.IsNullOrEmpty(updateRoomDTO.RoomNumber) && updateRoomDTO.RoomNumber != existingRoom.RoomNumber)
                {
                    var roomNumberExists = await GetRoomByRoomNumberAsync(updateRoomDTO.RoomNumber);
                    if (roomNumberExists.Success && roomNumberExists.Data != null)
                    {
                        response.Success = false;
                        response.Message = "Room with this room number already exists";
                        response.Data = null;
                        return response;
                    }
                }

                // Update properties
                existingRoom.RoomNumber = updateRoomDTO.RoomNumber ?? existingRoom.RoomNumber;
                existingRoom.Price = updateRoomDTO.Price ?? existingRoom.Price;
                existingRoom.MaxOccupancy = updateRoomDTO.MaxOccupancy ?? existingRoom.MaxOccupancy;
                existingRoom.Floor = updateRoomDTO.Floor ?? existingRoom.Floor;
                existingRoom.NumberOfBeds = updateRoomDTO.NumberOfBeds ?? existingRoom.NumberOfBeds;
                existingRoom.ACType = updateRoomDTO.AcType ?? existingRoom.ACType;
                existingRoom.BathroomType = updateRoomDTO.BathroomType ?? existingRoom.BathroomType;
                existingRoom.RoomStatus = updateRoomDTO.RoomStatus ?? existingRoom.RoomStatus;
                existingRoom.RoomType = updateRoomDTO.RoomType ?? existingRoom.RoomType;
                existingRoom.IsActive = updateRoomDTO.IsActive ?? existingRoom.IsActive;
                existingRoom.Description = updateRoomDTO.Description ?? existingRoom.Description;
                existingRoom.IsTv = updateRoomDTO.IsTv ?? existingRoom.IsTv;
                
                //// Handle Images
                //if (updateRoomDTO.Images != null)
                //{
                //    if (updateRoomDTO.Images.Count > 0)
                //    {
                //        // Filter out any null or empty strings
                //        var validImages = updateRoomDTO.Images.Where(img => !string.IsNullOrWhiteSpace(img)).ToList();
                //        if (validImages.Any())
                //        {
                //            existingRoom.Images = JsonSerializer.Serialize(validImages);
                //        }
                //        else
                //        {
                //            existingRoom.Images = string.Empty; // All provided images were invalid/empty
                //        }
                //    }
                //    else
                //    {
                //         existingRoom.Images = string.Empty; // Explicitly passed empty list -> remove logic? 
                //         // DTO comment said "empty list = remove all images"
                //    }
                //}
                //// If updateRoomDTO.Images is null, we do nothing and preserve existingRoom.Images

                // Handle Documents - Append new documents
                if (updateRoomDTO.Documents != null && updateRoomDTO.Documents.Any())
                {
                    string baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7252";
                    foreach (var doc in updateRoomDTO.Documents)
                    {
                        if (doc.File != null && doc.File.Length > 0)
                        {
                            string filePath = await Stayvelle.Services.Uploads.UploadImage(doc.FileName, doc.File, "RoomDocuments", baseUrl);
                            
                            var document = new DocumentModel
                            {
                                EntityType = "ROOM",
                                EntityId = existingRoom.Id,
                                DocumentType = doc.DocumentType ?? "ROOM_IMAGE",
                                FileName = doc.FileName,
                                Description = doc.Description,
                                FilePath = filePath,
                                IsPrimary = doc.IsPrimary,
                                CreatedBy = updateRoomDTO.ModifiedBy,
                                CreatedOn = DateTime.UtcNow,
                                ModifiedOn = DateTime.UtcNow
                            };
                            
                            _context.DocumentModel.Add(document);
                        }
                    }
                }

                existingRoom.ModifiedBy = updateRoomDTO.ModifiedBy;
                existingRoom.ModifiedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Populate documents for response
                var savedDocuments = await _context.DocumentModel
                    .Where(d => d.EntityType == "ROOM" && d.EntityId == existingRoom.Id)
                    .Select(d => new DocumentDto
                    {
                        DocumentId = d.DocumentId,
                        EntityType = d.EntityType,
                        EntityId = d.EntityId,
                        DocumentType = d.DocumentType,
                        FileName = d.FileName,
                        Description = d.Description,
                        FilePath = d.FilePath,
                        IsPrimary = d.IsPrimary
                    }).ToListAsync();

                existingRoom.Documents = savedDocuments;

                response.Success = true;
                response.Message = "Room updated successfully";
                response.Data = existingRoom;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error while updating room: " + ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Delete - Hard Delete
        public async Task<bool> DeleteRoomAsync(int id)
        {
            try
            {
                var room = await _context.RoomModel.FindAsync(id);
                if (room == null)
                {
                    return false;
                }

                _context.RoomModel.Remove(room);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Delete - Soft Delete (Sets IsActive to false)
        public async Task<bool> SoftDeleteRoomAsync(int id)
        {
            try
            {
                var room = await _context.RoomModel.FindAsync(id);
                if (room == null)
                {
                    return false;
                }

                room.IsActive = false;
                room.ModifiedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        // Read - Get Room By QR Token
        public async Task<Response<RoomModel?>> GetRoomByQrTokenAsync(string token)
        {
            var response = new Response<RoomModel?>();
            try
            {
                var room = await _context.RoomModel
                    .FirstOrDefaultAsync(r => r.RoomQrToken == token);

                if (room != null)
                {
                    // Populate Documents
                    var documents = await _context.DocumentModel
                        .Where(d => d.EntityType == "ROOM" && d.EntityId == room.Id)
                        .ToListAsync();

                    room.Documents = documents.Select(d => new DocumentDto
                    {
                        DocumentId = d.DocumentId,
                        EntityType = d.EntityType,
                        EntityId = d.EntityId,
                        DocumentType = d.DocumentType,
                        FileName = d.FileName,
                        Description = d.Description,
                        FilePath = d.FilePath,
                        IsPrimary = d.IsPrimary
                    }).ToList();
                }

                response.Success = room != null;
                response.Message = room != null ? "Success" : "Room not found";
                response.Data = room;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }
    }
}

