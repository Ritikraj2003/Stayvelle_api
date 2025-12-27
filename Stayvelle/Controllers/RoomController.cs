using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Query;
using System.Text.Json;

namespace Stayvelle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        public async Task<ActionResult<RoomModel>> CreateRoom([FromBody] CreateRoomDTO createRoomDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if room number already exists
            var existingRoomResponse = await _roomRepository.GetRoomByRoomNumberAsync(createRoomDTO.RoomNumber);
            if (existingRoomResponse.Success && existingRoomResponse.Data != null)
            {
                return Conflict(new { message = "Room with this room number already exists" });
            }

            // Convert Images list to JSON string (handle multiple images)
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
                AcType = createRoomDTO.AcType,
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

            var response = await _roomRepository.CreateRoomAsync(room);
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }
            return CreatedAtAction(nameof(GetRoom), new { id = response.Data.Id }, response.Data);
        }

        // PUT: api/Room/5
        [HttpPut("{id}")]
        public async Task<ActionResult<RoomModel>> UpdateRoom(int id, [FromBody] UpdateRoomDTO updateRoomDTO)
        {
            var existingRoomResponse = await _roomRepository.GetRoomByIdAsync(id);
            if (!existingRoomResponse.Success || existingRoomResponse.Data == null)
            {
                return NotFound(new { message = existingRoomResponse.Message ?? $"Room with ID {id} not found" });
            }

            var existingRoom = existingRoomResponse.Data;

            // Check if room number is being changed and already exists
            if (!string.IsNullOrEmpty(updateRoomDTO.RoomNumber) && updateRoomDTO.RoomNumber != existingRoom.RoomNumber)
            {
                var roomNumberExistsResponse = await _roomRepository.GetRoomByRoomNumberAsync(updateRoomDTO.RoomNumber);
                if (roomNumberExistsResponse.Success && roomNumberExistsResponse.Data != null)
                {
                    return Conflict(new { message = "Room with this room number already exists" });
                }
            }

            // Convert Images list to JSON string if provided (handle multiple images)
            string? imagesJson = null;
            if (updateRoomDTO.Images != null)
            {
                if (updateRoomDTO.Images.Count > 0)
                {
                    // Filter out any null or empty strings
                    var validImages = updateRoomDTO.Images.Where(img => !string.IsNullOrWhiteSpace(img)).ToList();
                    if (validImages.Any())
                    {
                        imagesJson = JsonSerializer.Serialize(validImages);
                    }
                    else
                    {
                        imagesJson = string.Empty; // All images were invalid, remove images
                    }
                }
                else
                {
                    imagesJson = string.Empty; // Empty list means remove all images
                }
            }
            // If Images is null, preserve existing images (don't set imagesJson)

            // Update only provided fields
            var roomToUpdate = new RoomModel
            {
                Id = id,
                RoomNumber = updateRoomDTO.RoomNumber ?? existingRoom.RoomNumber,
                Price = updateRoomDTO.Price ?? existingRoom.Price,
                MaxOccupancy = updateRoomDTO.MaxOccupancy ?? existingRoom.MaxOccupancy,
                Floor = updateRoomDTO.Floor ?? existingRoom.Floor,
                NumberOfBeds = updateRoomDTO.NumberOfBeds ?? existingRoom.NumberOfBeds,
                AcType = updateRoomDTO.AcType ?? existingRoom.AcType,
                BathroomType = updateRoomDTO.BathroomType ?? existingRoom.BathroomType,
                RoomStatus = updateRoomDTO.RoomStatus ?? existingRoom.RoomStatus,
                RoomType = updateRoomDTO.RoomType ?? existingRoom.RoomType,
                IsActive = updateRoomDTO.IsActive ?? existingRoom.IsActive,
                Description = updateRoomDTO.Description ?? existingRoom.Description,
                IsTv = updateRoomDTO.IsTv ?? existingRoom.IsTv,
                Images = imagesJson ?? existingRoom.Images, // If null, preserve existing; if provided, use new value
                ModifiedBy = existingRoom.ModifiedBy,
                ModifiedOn = DateTime.UtcNow
            };

            var updatedRoomResponse = await _roomRepository.UpdateRoomAsync(id, roomToUpdate);
            if (!updatedRoomResponse.Success || updatedRoomResponse.Data == null)
            {
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

