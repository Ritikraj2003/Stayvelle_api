using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.IRepository
{
    public interface IRolePermission
    {
        Task<Response<bool>> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds, string createdBy);
        Task<Response<bool>> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds, string modifiedBy);
        Task<Response<List<RolePermissionModel>>> GetRolePermissionsByRoleIdAsync(int roleId);
        Task<Response<bool>> RemovePermissionFromRoleAsync(int roleId, int permissionId);
    }
}

