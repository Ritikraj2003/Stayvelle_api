using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.IRepository
{
    public interface IHousekeepingTask
    {
        // Create
        Task<Response<HousekeepingTask>> CreateHousekeepingTaskAsync(HousekeepingTask task);

        // Read
        Task<Response<List<HousekeepingTask>>> GetAllHousekeepingTasksAsync();
        Task<Response<HousekeepingTask?>> GetHousekeepingTaskByIdAsync(int taskId);
        Task<Response<List<HousekeepingTask>>> GetHousekeepingTasksByRoomIdAsync(int roomid);
        Task<Response<List<HousekeepingTask>>> GetHousekeepingTasksByUserIdAsync(int userId);

        // Update
        Task<Response<HousekeepingTask?>> UpdateHousekeepingTaskAsync(int taskId, HousekeepingTask task);
        Task<Response<HousekeepingTask?>> CompleteTaskAndSetRoomAvailableAsync(int taskId);
    }
}

