using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.IRepository
{
    public interface IRoom
    {
        // Create
        Task<Response<RoomModel>> CreateRoomAsync(CreateRoomDTO createRoomDTO);

        // Read
        Task<Response<List<RoomModel>>> GetAllRoomsAsync();
        Task<Response<RoomModel?>> GetRoomByIdAsync(int id);
        Task<Response<RoomModel?>> GetRoomByRoomNumberAsync(string roomNumber);
        Task<Response<List<RoomModel>>> GetRoomsByStatusAsync(string status);
        Task<Response<List<RoomModel>>> GetRoomsByTypeAsync(string roomType);
        Task<Response<RoomModel?>> GetRoomByQrTokenAsync(string token);

        // Update
        Task<Response<RoomModel?>> UpdateRoomAsync(int id, UpdateRoomDTO updateRoomDTO);

        // Delete
        Task<bool> DeleteRoomAsync(int id);
        Task<bool> SoftDeleteRoomAsync(int id); // Sets IsActive flag to false
    }
}

