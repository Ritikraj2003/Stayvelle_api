using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.IRepository
{
    public interface IRole
    {
        Task<Response<RoleModel>> CreateRoleAsync(RoleModel role);
        Task<Response<List<RoleModel>>> GetAllRolesAsync();
        Task<Response<RoleModel?>> GetRoleByIdAsync(int id);
        Task<Response<RoleModel?>> GetRoleByNameAsync(string name);
        Task<Response<RoleModel?>> UpdateRoleAsync(int id, RoleModel role);
        Task<bool> DeleteRoleAsync(int id);
        Task<bool> SoftDeleteRoleAsync(int id);
    }
}

