using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class RoomController : ControllerBase
    {
        private readonly IRoom _roomRepository;

        public RoomController(IRoom roomRepository)
        {
            _roomRepository = roomRepository;
        }

        // GET: api/Room
        [HttpGet]
        public async Task<ActionResult<List<RoomModel>>> GetAllRooms()
        {
            var response = await _roomRepository.GetAllRoomsAsync();
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }
            
            // Images are stored as JSON string, frontend can deserialize using JsonSerializer.Deserialize<List<string>>(room.Images)
            return Ok(response.Data);
        }

        // GET: api/Room/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomModel>> GetRoom(int id)
        {
            var response = await _roomRepository.GetRoomByIdAsync(id);
            if (!response.Success || response.Data == null)
            {
                return NotFound(new { message = response.Message ?? $"Room with ID {id} not found" });
            }
            
            // Images are stored as JSON string, frontend can deserialize using JsonSerializer.Deserialize<List<string>>(room.Images)
            return Ok(response.Data);
        }

        // GET: api/Room/roomnumber/{roomNumber}
        [HttpGet("roomnumber/{roomNumber}")]
        public async Task<ActionResult<RoomModel>> GetRoomByRoomNumber(string roomNumber)
        {
            var response = await _roomRepository.GetRoomByRoomNumberAsync(roomNumber);
            if (!response.Success || response.Data == null)
            {
                return NotFound(new { message = response.Message ?? $"Room with number {roomNumber} not found" });
            }
            return Ok(response.Data);
        }

        // GET: api/Room/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<RoomModel>>> GetRoomsByStatus(string status)
        {
            var response = await _roomRepository.GetRoomsByStatusAsync(status);
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(response.Data);
        }

        // GET: api/Room/type/{roomType}
        [HttpGet("type/{roomType}")]
        public async Task<ActionResult<List<RoomModel>>> GetRoomsByType(string roomType)
        {
            var response = await _roomRepository.GetRoomsByTypeAsync(roomType);
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(response.Data);
        }

        // POST: api/Room
        [HttpPost]
        public async Task<ActionResult<RoomModel>> CreateRoom([FromForm] CreateRoomDTO createRoomDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _roomRepository.CreateRoomAsync(createRoomDTO);
            if (!response.Success || response.Data == null)
            {
                if (response.Message != null && response.Message.Contains("already exists"))
                {
                     return Conflict(new { message = response.Message });
                }
                return BadRequest(new { message = response.Message });
            }
            return CreatedAtAction(nameof(GetRoom), new { id = response.Data.Id }, response.Data);
        }

        // PUT: api/Room/5
        [HttpPut("{id}")]
        public async Task<ActionResult<RoomModel>> UpdateRoom(int id, [FromForm] UpdateRoomDTO updateRoomDTO)
        {
            var updatedRoomResponse = await _roomRepository.UpdateRoomAsync(id, updateRoomDTO);
            if (!updatedRoomResponse.Success || updatedRoomResponse.Data == null)
            {
                 if (updatedRoomResponse.Message != null && updatedRoomResponse.Message.Contains("already exists"))
                {
                     return Conflict(new { message = updatedRoomResponse.Message });
                }
                if (updatedRoomResponse.Message != null && updatedRoomResponse.Message.Contains("not found"))
                {
                    return NotFound(new { message = updatedRoomResponse.Message });
                }
                return BadRequest(new { message = updatedRoomResponse.Message ?? $"Error updating room with ID {id}" });
            }

            return Ok(updatedRoomResponse.Data);
        }

        // DELETE: api/Room/5 (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRoom(int id)
        {
            var result = await _roomRepository.SoftDeleteRoomAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Room with ID {id} not found" });
            }
            return Ok(new { message = $"Room with ID {id} deleted successfully" });
        }

        // DELETE: api/Room/5/hard (Hard Delete - permanently remove)
        [HttpDelete("{id}/hard")]
        public async Task<ActionResult> HardDeleteRoom(int id)
        {
            var result = await _roomRepository.DeleteRoomAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Room with ID {id} not found" });
            }
            return Ok(new { message = $"Room with ID {id} permanently deleted" });
        }
    }
}

