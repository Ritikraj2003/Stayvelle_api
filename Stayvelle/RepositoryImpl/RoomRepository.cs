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
        public async Task<Response<RoomModel>> CreateRoomAsync(RoomModel room)
        {
            Response<RoomModel> response = new Response<RoomModel>();
            try
            {
                // Check if room number already exists
                var existingRoom = await _context.RoomModel
                    .FirstOrDefaultAsync(r => r.RoomNumber == room.RoomNumber);
                
                if (existingRoom != null)
                {
                    response.Success = false;
                    response.Message = "Room with this room number already exists";
                    response.Data = null;
                    return response;
                }

                // Convert Images list to JSON string if provided
                // Images should come as JSON string from controller, but handle both cases
                if (room.Images != null && !string.IsNullOrEmpty(room.Images))
                {
                    // Validate it's valid JSON array, if not, try to fix it
                    try
                    {
                        var imagesList = JsonSerializer.Deserialize<List<string>>(room.Images);
                        if (imagesList == null || !imagesList.Any())
                        {
                            room.Images = null;
                        }
                    }
                    catch
                    {
                        // If not valid JSON array, assume it's a single image string and wrap it in array
                        var imageList = new List<string> { room.Images };
                        room.Images = JsonSerializer.Serialize(imageList);
                    }
                }

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
        public async Task<Response<RoomModel?>> UpdateRoomAsync(int id, RoomModel room)
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
                if (!string.IsNullOrEmpty(room.RoomNumber) && room.RoomNumber != existingRoom.RoomNumber)
                {
                    var roomNumberExists = await GetRoomByRoomNumberAsync(room.RoomNumber);
                    if (roomNumberExists.Success && roomNumberExists.Data != null)
                    {
                        response.Success = false;
                        response.Message = "Room with this room number already exists";
                        response.Data = null;
                        return response;
                    }
                }

                // Update properties
                existingRoom.RoomNumber = room.RoomNumber ?? existingRoom.RoomNumber;
                existingRoom.Price = room.Price != 0 ? room.Price : existingRoom.Price;
                existingRoom.MaxOccupancy = room.MaxOccupancy != 0 ? room.MaxOccupancy : existingRoom.MaxOccupancy;
                existingRoom.Floor = room.Floor ?? existingRoom.Floor;
                existingRoom.NumberOfBeds = room.NumberOfBeds ?? existingRoom.NumberOfBeds;
                existingRoom.AcType = room.AcType ?? existingRoom.AcType;
                existingRoom.BathroomType = room.BathroomType ?? existingRoom.BathroomType;
                existingRoom.RoomStatus = room.RoomStatus ?? existingRoom.RoomStatus;
                existingRoom.RoomType = room.RoomType ?? existingRoom.RoomType;
                existingRoom.IsActive = room.IsActive;
                existingRoom.Description = room.Description ?? existingRoom.Description;
                existingRoom.IsTv = room.IsTv;
                
                // Handle Images
                if (room.Images != null)
                {
                    if (string.IsNullOrWhiteSpace(room.Images))
                    {
                        existingRoom.Images = null; // Remove images
                    }
                    else
                    {
                        // Try to parse as JSON, if not valid, serialize it
                        try
                        {
                            JsonSerializer.Deserialize<List<string>>(room.Images);
                            existingRoom.Images = room.Images; // Already valid JSON
                        }
                        catch
                        {
                            var imageList = new List<string> { room.Images };
                            existingRoom.Images = JsonSerializer.Serialize(imageList);
                        }
                    }
                }

                existingRoom.ModifiedBy = room.ModifiedBy ?? existingRoom.ModifiedBy;
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

