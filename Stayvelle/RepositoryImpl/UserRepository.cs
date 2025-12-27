using Microsoft.EntityFrameworkCore;
using Npgsql;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.RepositoryImpl
{
    public class UserRepository : IUsers
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Create
        public async Task<Response<UsersModel>> CreateUserAsync(UsersModel user)
        {
            Response<UsersModel> response = new Response<UsersModel>();
            try
            {
                _context.UsersModel.Add(user);
                await _context.SaveChangesAsync();

                response.Success = true;
                response.Message = "User created successfully";
                response.Data = user;

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

        // Read - Get All Users
        public async Task<Response<List<UsersModel>>> GetAllUsersAsync()
        {
            var response = new Response<List<UsersModel>>();
            try
            {
               var res = await _context.UsersModel.FromSqlRaw(UserQuery.GetAllUsers).ToListAsync();
                response.Success = true;
                response.Message = "success";
                response.Data = res;
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

        public async Task<Response<UsersModel>> GetUserByIdAsync(int id)
        {
            var response = new Response<UsersModel>();

            try
            {
                var user = await _context.UsersModel.FindAsync(id);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    response.Data = null;
                    return response;
                }

                response.Success = true;
                response.Message = "Success";
                response.Data = user;
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

        public async Task<Response<UsersModel?>> GetUserByEmailAsync(string email)
        {
            var response = new Response<UsersModel>();
            try
            {
                response.Data = await _context.UsersModel.FirstOrDefaultAsync(u => u.Email == email);
                response.Success = true;
                response.Message = "success";
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

        public async Task<UsersModel?> GetUserByUsernameAsync(string username)
        {
            return await _context.UsersModel.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<Response<UsersModel>> UpdateUserAsync(int id, UsersModel user)
        {
            var response = new Response<UsersModel>();

            try
            {
                var existingUserResponse = await GetUserByIdAsync(id);

                if (!existingUserResponse.Success || existingUserResponse.Data == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    response.Data = null;
                    return response;
                }
                var parameters = new[]
                {
                    new NpgsqlParameter("@Id", id),
                    new NpgsqlParameter("@Name", user.Name ?? (object)DBNull.Value),
                    new NpgsqlParameter("@Email", user.Email ?? (object)DBNull.Value),
                    new NpgsqlParameter("@Password", user.Password ?? (object)DBNull.Value),
                    new NpgsqlParameter("@Username", user.Username ?? (object)DBNull.Value),
                    new NpgsqlParameter("@Phone", user.Phone ?? (object)DBNull.Value),
                    new NpgsqlParameter("@isactive", user.isactive),
                    new NpgsqlParameter("@isstaff", user.isstaff),
                    new NpgsqlParameter("@isadmin", user.isadmin),
                    new NpgsqlParameter("@role_id", user.role_id),
                    new NpgsqlParameter("@role_name", user.role_name ?? (object)DBNull.Value),
                    new NpgsqlParameter("@ImageUrl", user.ImageUrl ?? (object)DBNull.Value)
                };
                await _context.Database.ExecuteSqlRawAsync(UserQuery.UpdateUser, parameters);
                return await GetUserByIdAsync(id);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error while updating user"; // don't expose ex.Message
                response.Data = null;
                return response;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var userIdParam = new NpgsqlParameter("@Id", id);
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(UserQuery.HardDeleteUser, userIdParam);
            return rowsAffected > 0;
        }

        public async Task<bool> SoftDeleteUserAsync(int id)
        {
            var userIdParam = new NpgsqlParameter("@Id", id);
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(UserQuery.SoftDeleteUser, userIdParam);
            return rowsAffected > 0;
        }
    }
}
