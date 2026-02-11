using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Query;
using System.Text.Json;

namespace Stayvelle.RepositoryImpl
{
    public class RoomRepository : IRoom
    {
        private readonly ApplicationDbContext _context;

        public RoomRepository(ApplicationDbContext context)
        {
            _context = context;
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
                string? imagesJson = null;
                if (createRoomDTO.Images != null && createRoomDTO.Images.Count > 0)
                {
                    // Filter out any null or empty strings
                    var validImages = createRoomDTO.Images.Where(img => !string.IsNullOrWhiteSpace(img)).ToList();
                    if (validImages.Any())
                    {
                        imagesJson = JsonSerializer.Serialize(validImages);
                    }
                }

                var room = new RoomModel
                {
                    RoomNumber = createRoomDTO.RoomNumber,
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
                    Images = imagesJson,
                    CreatedBy = createRoomDTO.CreatedBy,
                    CreatedOn = createRoomDTO.CreatedOn,
                    ModifiedBy = createRoomDTO.ModifiedBy,
                    ModifiedOn = createRoomDTO.ModifiedOn
                };

                _context.RoomModel.Add(room);
                await _context.SaveChangesAsync();

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
                
                // Handle Images
                if (updateRoomDTO.Images != null)
                {
                    if (updateRoomDTO.Images.Count > 0)
                    {
                        // Filter out any null or empty strings
                        var validImages = updateRoomDTO.Images.Where(img => !string.IsNullOrWhiteSpace(img)).ToList();
                        if (validImages.Any())
                        {
                            existingRoom.Images = JsonSerializer.Serialize(validImages);
                        }
                        else
                        {
                            existingRoom.Images = string.Empty; // All provided images were invalid/empty
                        }
                    }
                    else
                    {
                         existingRoom.Images = string.Empty; // Explicitly passed empty list -> remove logic? 
                         // DTO comment said "empty list = remove all images"
                    }
                }
                // If updateRoomDTO.Images is null, we do nothing and preserve existingRoom.Images

                existingRoom.ModifiedBy = updateRoomDTO.ModifiedBy;
                existingRoom.ModifiedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();

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
    }
}

