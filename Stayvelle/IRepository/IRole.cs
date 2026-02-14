using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.IRepository
{
    public interface IRole
    {
        Task<Response<RoleResponseDTO>> CreateRoleAsync(CreateRoleDTO createRoleDTO);
        Task<Response<List<RoleResponseDTO>>> GetAllRolesAsync(string? search = null);
        Task<Response<RoleResponseDTO?>> GetRoleByIdAsync(int id);
        Task<Response<RoleResponseDTO?>> GetRoleByNameAsync(string name);
        Task<Response<RoleResponseDTO?>> UpdateRoleAsync(int id, UpdateRoleDTO updateRoleDTO);
        Task<bool> DeleteRoleAsync(int id);
        Task<bool> SoftDeleteRoleAsync(int id);
    }
}

