using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.IRepository
{
    public interface IRoom
    {
        // Create
        Task<Response<RoomModel>> CreateRoomAsync(RoomModel room);

        // Read
        Task<Response<List<RoomModel>>> GetAllRoomsAsync();
        Task<Response<RoomModel?>> GetRoomByIdAsync(int id);
        Task<Response<RoomModel?>> GetRoomByRoomNumberAsync(string roomNumber);
        Task<Response<List<RoomModel>>> GetRoomsByStatusAsync(string status);
        Task<Response<List<RoomModel>>> GetRoomsByTypeAsync(string roomType);

        // Update
        Task<Response<RoomModel?>> UpdateRoomAsync(int id, RoomModel room);

        // Delete
        Task<bool> DeleteRoomAsync(int id);
        Task<bool> SoftDeleteRoomAsync(int id); // Sets IsActive flag to false
    }
}

