using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.RepositoryImpl
{
    public class HousekeepingTaskRepository : IHousekeepingTask
    {
        private readonly ApplicationDbContext _context;

        public HousekeepingTaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Create
        public async Task<Response<HousekeepingTask>> CreateHousekeepingTaskAsync(HousekeepingTask task)
        {
            var response = new Response<HousekeepingTask>();
            try
            {
                // Validate room exists
                var room = await _context.RoomModel.FindAsync(task.RoomId);
                if (room == null)
                {
                    response.Success = false;
                    response.Message = "Room not found";
                    response.Data = null;
                    return response;
                }

                // Validate booking exists
                var booking = await _context.BookingModel.FindAsync(task.BookingId);
                if (booking == null)
                {
                    response.Success = false;
                    response.Message = "Booking not found";
                    response.Data = null;
                    return response;
                }

                // Set CommonModel fields if not already set
                if (string.IsNullOrEmpty(task.CreatedBy))
                {
                    task.CreatedBy = "system";
                }
                if (task.CreatedOn == default(DateTime))
                {
                    task.CreatedOn = DateTime.UtcNow;
                }
                else
                {
                    task.CreatedOn = task.CreatedOn.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(task.CreatedOn, DateTimeKind.Utc)
                        : task.CreatedOn.ToUniversalTime();
                }

                task.TaskType = task.TaskType ?? "Cleaning";
                task.TaskStatus = task.TaskStatus ?? "Pending";

                _context.HousekeepingTask.Add(task);
                await _context.SaveChangesAsync();

                // Reload with related data
                var savedTask = await _context.HousekeepingTask
                    .Include(t => t.Room)
                    .Include(t => t.Booking)
                    .FirstOrDefaultAsync(t => t.TaskId == task.TaskId);

                response.Success = true;
                response.Message = "Housekeeping task created successfully";
                response.Data = savedTask ?? task;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error creating housekeeping task: " + ex.Message;
                response.Data = null;
                Console.WriteLine($"Error creating housekeeping task: {ex}");
                return response;
            }
        }

        // Read - Get All Tasks
        public async Task<Response<List<HousekeepingTask>>> GetAllHousekeepingTasksAsync()
        {
            var response = new Response<List<HousekeepingTask>>();
            try
            {
                var tasks = await _context.HousekeepingTask
                    //.Include(t => t.Room)
                    //.Include(t => t.Booking)
                    .OrderByDescending(t => t.CreatedOn)
                    .ToListAsync();

                response.Success = true;
                response.Message = "Success";
                response.Data = tasks;
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

        // Read - Get Task By Id
        public async Task<Response<HousekeepingTask?>> GetHousekeepingTaskByIdAsync(int taskId)
        {
            var response = new Response<HousekeepingTask?>();
            try
            {
                var task = await _context.HousekeepingTask
                    .Include(t => t.Room)
                    .Include(t => t.Booking)
                    .FirstOrDefaultAsync(t => t.TaskId == taskId);

                if (task == null)
                {
                    response.Success = false;
                    response.Message = "Housekeeping task not found";
                    response.Data = null;
                    return response;
                }

                response.Success = true;
                response.Message = "Success";
                response.Data = task;
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

        // Read - Get Tasks By Booking Id
        public async Task<Response<List<HousekeepingTask>>> GetHousekeepingTasksByRoomIdAsync(int roomid)
        {
            var response = new Response<List<HousekeepingTask>>();
            try
            {
                var tasks = await _context.HousekeepingTask
                    //.Include(t => t.Room)
                    //.Include(t => t.Booking)
                    .Where(t => t.RoomId == roomid)
                    .OrderByDescending(t => t.CreatedOn)
                    .ToListAsync();

                response.Success = true;
                response.Message = "Success";
                response.Data = tasks;
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

        // Read - Get Tasks By User Id
        public async Task<Response<List<HousekeepingTask>>> GetHousekeepingTasksByUserIdAsync(int userId)
        {
            var response = new Response<List<HousekeepingTask>>();
            try
            {
                var tasks = await _context.HousekeepingTask
                    .Include(t => t.Room)
                    .Include(t => t.Booking)
                    .Where(t => t.AssignedToUserId == userId)
                    .OrderByDescending(t => t.CreatedOn)
                    .ToListAsync();

                response.Success = true;
                response.Message = "Success";
                response.Data = tasks;
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
        public async Task<Response<HousekeepingTask?>> UpdateHousekeepingTaskAsync(int taskId, HousekeepingTask task)
        {
            var response = new Response<HousekeepingTask?>();
            try
            {
                var existingTask = await _context.HousekeepingTask
                    .FirstOrDefaultAsync(t => t.TaskId == taskId);

                if (existingTask == null)
                {
                    response.Success = false;
                    response.Message = "Housekeeping task not found";
                    response.Data = null;
                    return response;
                }

                existingTask.RoomId = task.RoomId;
                existingTask.BookingId = task.BookingId;
                existingTask.TaskType = task.TaskType;
                existingTask.TaskStatus = task.TaskStatus;
                existingTask.RoomImage = task.RoomImage;
                existingTask.AssignedToUserId = task.AssignedToUserId;
                existingTask.ModifiedBy = task.ModifiedBy ?? existingTask.ModifiedBy;
                existingTask.ModifiedOn = DateTime.UtcNow;

                // Update room status to "Maintenance"
                if (existingTask.Room.RoomStatus == "Maintenance" && existingTask.TaskStatus== "Completed")
                {
                    existingTask.Room.RoomStatus = "Available";
                }

                await _context.SaveChangesAsync();

                // Reload with includes
                await _context.Entry(existingTask)
                    .Reference(t => t.Room)
                    .LoadAsync();
                await _context.Entry(existingTask)
                    .Reference(t => t.Booking)
                    .LoadAsync();

                response.Success = true;
                response.Message = "Housekeeping task updated successfully";
                response.Data = existingTask;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error updating housekeeping task: " + ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Complete Task and Set Room Available
        public async Task<Response<HousekeepingTask?>> CompleteTaskAndSetRoomAvailableAsync(int taskId)
        {
            var response = new Response<HousekeepingTask?>();
            try
            {
                var task = await _context.HousekeepingTask
                    .Include(t => t.Room)
                    .Include(t => t.Booking)
                    .FirstOrDefaultAsync(t => t.TaskId == taskId);

                if (task == null)
                {
                    response.Success = false;
                    response.Message = "Housekeeping task not found";
                    response.Data = null;
                    return response;
                }

                // Update task status to Completed
                task.TaskStatus = "Completed";
                task.ModifiedOn = DateTime.UtcNow;

                // Update room status to Available
                if (task.Room == null)
                {
                    task.Room.RoomStatus = "Available";
                }

                await _context.SaveChangesAsync();

                response.Success = true;
                response.Message = "Task completed and room set to available";
                response.Data = task;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error completing task: " + ex.Message;
                response.Data = null;
                return response;
            }
        }
    }
}

