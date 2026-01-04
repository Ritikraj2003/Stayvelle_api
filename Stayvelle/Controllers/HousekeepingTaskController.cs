using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stayvelle.IRepository;
using Stayvelle.Models;

namespace Stayvelle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class HousekeepingTaskController : ControllerBase
    {
        private readonly IHousekeepingTask _housekeepingTaskRepository;

        public HousekeepingTaskController(IHousekeepingTask housekeepingTaskRepository)
        {
            _housekeepingTaskRepository = housekeepingTaskRepository;
        }

        // GET: api/HousekeepingTask
        [HttpGet]
        public async Task<ActionResult<List<HousekeepingTask>>> GetAllHousekeepingTasks()
        {
            var response = await _housekeepingTaskRepository.GetAllHousekeepingTasksAsync();
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(response.Data);
        }

        // GET: api/HousekeepingTask/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HousekeepingTask>> GetHousekeepingTask(int id)
        {
            var response = await _housekeepingTaskRepository.GetHousekeepingTaskByIdAsync(id);
            if (!response.Success || response.Data == null)
            {
                return NotFound(new { message = response.Message ?? $"Housekeeping task with ID {id} not found" });
            }
            return Ok(response.Data);
        }

        // GET: api/HousekeepingTask/booking/5
        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<List<HousekeepingTask>>> GetHousekeepingTasksByBookingId(int bookingId)
        {
            var response = await _housekeepingTaskRepository.GetHousekeepingTasksByBookingIdAsync(bookingId);
            if (!response.Success || response.Data == null)
            {
                return NotFound(new { message = response.Message ?? $"No housekeeping tasks found for booking ID {bookingId}" });
            }
            return Ok(response.Data);
        }

        // GET: api/HousekeepingTask/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<HousekeepingTask>>> GetHousekeepingTasksByUserId(int userId)
        {
            var response = await _housekeepingTaskRepository.GetHousekeepingTasksByUserIdAsync(userId);
            if (!response.Success || response.Data == null)
            {
                return NotFound(new { message = response.Message ?? $"No housekeeping tasks found for user ID {userId}" });
            }
            return Ok(response.Data);
        }

        // POST: api/HousekeepingTask
        [HttpPost]
        public async Task<ActionResult<HousekeepingTask>> CreateHousekeepingTask([FromBody] HousekeepingTask task)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _housekeepingTaskRepository.CreateHousekeepingTaskAsync(task);
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }

            return CreatedAtAction(nameof(GetHousekeepingTask), new { id = response.Data.TaskId }, response.Data);
        }

        // PUT: api/HousekeepingTask/5
        [HttpPut("{id}")]
        public async Task<ActionResult<HousekeepingTask>> UpdateHousekeepingTask(int id, [FromBody] HousekeepingTask task)
        {
            var existingTaskResponse = await _housekeepingTaskRepository.GetHousekeepingTaskByIdAsync(id);
            if (!existingTaskResponse.Success || existingTaskResponse.Data == null)
            {
                return NotFound(new { message = existingTaskResponse.Message ?? $"Housekeeping task with ID {id} not found" });
            }

            // Get existing task to preserve values that aren't being updated
            var existingTask = existingTaskResponse.Data;
            
            // Only update fields that are provided (not null/empty)
            if (task.RoomId > 0) existingTask.RoomId = task.RoomId;
            if (task.BookingId > 0) existingTask.BookingId = task.BookingId;
            if (!string.IsNullOrEmpty(task.TaskType)) existingTask.TaskType = task.TaskType;
            if (!string.IsNullOrEmpty(task.TaskStatus)) existingTask.TaskStatus = task.TaskStatus;
            if (task.RoomImage != null) existingTask.RoomImage = task.RoomImage;
            if (task.AssignedToUserId.HasValue) existingTask.AssignedToUserId = task.AssignedToUserId;
            
            existingTask.ModifiedOn = DateTime.UtcNow;

            var updatedTaskResponse = await _housekeepingTaskRepository.UpdateHousekeepingTaskAsync(id, existingTask);
            if (!updatedTaskResponse.Success || updatedTaskResponse.Data == null)
            {
                return BadRequest(new { message = updatedTaskResponse.Message ?? $"Error updating housekeeping task with ID {id}" });
            }

            return Ok(updatedTaskResponse.Data);
        }

        // POST: api/HousekeepingTask/5/complete
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<HousekeepingTask>> CompleteTaskAndSetRoomAvailable(int id)
        {
            var response = await _housekeepingTaskRepository.CompleteTaskAndSetRoomAvailableAsync(id);
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message ?? $"Error completing housekeeping task {id}" });
            }
            return Ok(response.Data);
        }
    }
}

