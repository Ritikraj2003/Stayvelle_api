using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.IRepository
{
    public interface IPermission
    {
        Task<Response<PermissionModel>> CreatePermissionAsync(PermissionModel permission);
        Task<Response<List<PermissionModel>>> GetAllPermissionsAsync();
        Task<Response<PermissionModel?>> GetPermissionByIdAsync(int id);
        Task<Response<List<PermissionModel>>> GetPermissionsByRoleIdAsync(int roleId);
        Task<Response<PermissionModel?>> UpdatePermissionAsync(int id, PermissionModel permission);
        Task<bool> DeletePermissionAsync(int id);
        Task<bool> SoftDeletePermissionAsync(int id);
    }
}

