using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stayvelle.Models;
using Stayvelle.Models.DTOs;
using Stayvelle.IRepository;
using Stayvelle.RepositoryImpl;

namespace Stayvelle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IMasterServices _masterServicesRepository; // Use Interface

        public ServiceController(IMasterServices masterServicesRepository)
        {
            _masterServicesRepository = masterServicesRepository;
        }

        [HttpGet("GetAllServices")]
        public async Task<ActionResult<List<ServiceResponseDto>>> GetAllServcie()
        {
            var response = await _masterServicesRepository.GetAllServicesAsync();
            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(response);
        }

        [HttpGet("GetServiceById/{id}")]
        public async Task<ActionResult<ServiceResponseDto>> GetServiceById(int id)
        {
            var response = await _masterServicesRepository.GetServiceByIdAsync(id);
            if (!response.Success)
            {
                return NotFound(new { message = response.Message });
            }
            return Ok(response);
        }

        [HttpPost("CreateService")]
        public async Task<ActionResult<ServiceModel>> CreateService([FromForm] CreateServiceDto createServiceDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _masterServicesRepository.CreateServiceAsync(createServiceDto);
            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(response);
        }

        [HttpPut("UpdateService/{id}")]
        public async Task<ActionResult<ServiceModel>> UpdateService(int id, [FromForm] UpdateServiceDto updateServiceDto)
        {
            if (id != updateServiceDto.ServiceId)
            {
                return BadRequest("Service ID mismatch");
            }

            var response = await _masterServicesRepository.UpdateServiceAsync(id, updateServiceDto);
            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(response);
        }

        [HttpDelete("DeleteService/{id}")]
        public async Task<ActionResult<bool>> DeleteService(int id)
        {
            var response = await _masterServicesRepository.DeleteServiceAsync(id);
            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(response);
        }
    }
}
