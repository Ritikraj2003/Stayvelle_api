using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.IRepository
{
    public interface IMasterServices
    {
        Task<Response<List<ServiceResponseDto>>> GetAllServicesAsync();
        Task<Response<ServiceResponseDto?>> GetServiceByIdAsync(int serviceId);
        Task<Response<ServiceModel>> CreateServiceAsync(CreateServiceDto createServiceDto);
        Task<Response<ServiceModel>> UpdateServiceAsync(int serviceId, UpdateServiceDto updateServiceDto);
        Task<Response<bool>> DeleteServiceAsync(int serviceId);
    }
}
