using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.RepositoryImpl
{
    public class RoleRepository : IRole
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<RoleModel>> CreateRoleAsync(RoleModel role)
        {
            var response = new Response<RoleModel>();
            try
            {
                _context.RoleModel.Add(role);
                await _context.SaveChangesAsync();

                response.Success = true;
                response.Message = "Role created successfully";
                response.Data = role;
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

        public async Task<Response<List<RoleModel>>> GetAllRolesAsync()
        {
            var response = new Response<List<RoleModel>>();
            try
            {
                var roles = await _context.RoleModel
                    .Where(r => !r.isdelete)
                    .ToListAsync();
                
                response.Success = true;
                response.Message = "Success";
                response.Data = roles;
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

        public async Task<Response<RoleModel?>> GetRoleByIdAsync(int id)
        {
            var response = new Response<RoleModel?>();
            try
            {
                var role = await _context.RoleModel
                    .FirstOrDefaultAsync(r => r.Id == id && !r.isdelete);

                if (role == null)
                {
                    response.Success = false;
                    response.Message = "Role not found";
                    response.Data = null;
                    return response;
                }

                response.Success = true;
                response.Message = "Success";
                response.Data = role;
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

        public async Task<Response<RoleModel?>> GetRoleByNameAsync(string name)
        {
            var response = new Response<RoleModel?>();
            try
            {
                var role = await _context.RoleModel
                    .FirstOrDefaultAsync(r => r.role_name == name && !r.isdelete);

                response.Success = true;
                response.Message = "Success";
                response.Data = role;
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

        public async Task<Response<RoleModel?>> UpdateRoleAsync(int id, RoleModel role)
        {
            var response = new Response<RoleModel?>();
            try
            {
                var existingRole = await _context.RoleModel.FindAsync(id);
                if (existingRole == null || existingRole.isdelete)
                {
                    response.Success = false;
                    response.Message = "Role not found";
                    response.Data = null;
                    return response;
                }

                existingRole.role_name = role.role_name;
                existingRole.isactive = role.isactive;
                existingRole.ModifiedBy = role.ModifiedBy;
                existingRole.ModifiedOn = role.ModifiedOn;

                await _context.SaveChangesAsync();
                return await GetRoleByIdAsync(id);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            try
            {
                var role = await _context.RoleModel.FindAsync(id);
                if (role == null) return false;

                _context.RoleModel.Remove(role);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SoftDeleteRoleAsync(int id)
        {
            try
            {
                var role = await _context.RoleModel.FindAsync(id);
                if (role == null || role.isdelete) return false;

                role.isdelete = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

