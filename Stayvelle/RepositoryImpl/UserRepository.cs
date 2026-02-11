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
        public async Task<Response<UsersModel>> CreateUserAsync(CreateUserDTO createUserDTO)
        {
            Response<UsersModel> response = new Response<UsersModel>();
            try
            {
                // Check if email already exists
                var existingEmail = await _context.UsersModel.FirstOrDefaultAsync(u => u.Email == createUserDTO.Email);
                if (existingEmail != null)
                {
                    response.Success = false;
                    response.Message = "User with this email already exists";
                    return response;
                }

                // Check if username already exists
                var existingUsername = await _context.UsersModel.FirstOrDefaultAsync(u => u.Username == createUserDTO.Username);
                if (existingUsername != null)
                {
                    response.Success = false;
                    response.Message = "User with this username already exists";
                    return response;
                }

                var user = new UsersModel
                {
                    Name = createUserDTO.Name,
                    Email = createUserDTO.Email,
                    Password = createUserDTO.Password,
                    Username = createUserDTO.Username,
                    isactive = createUserDTO.isactive,
                    isstaff = createUserDTO.isstaff,
                    isadmin = createUserDTO.isadmin,
                    isdelete = false,
                    Phone = createUserDTO.Phone,
                    role_id = createUserDTO.role_id,
                    role_name = createUserDTO.role_name,
                    ImageUrl = createUserDTO.ImageUrl,
                    CreatedBy = createUserDTO.CreatedBy,
                    CreatedOn = DateTime.UtcNow
                };

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

        public async Task<Response<UsersModel>> UpdateUserAsync(int id, UpdateUserDTO updateUserDTO)
        {
            var response = new Response<UsersModel>();

            try
            {
                var existingUser = await _context.UsersModel.FindAsync(id);

                if (existingUser == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    response.Data = null;
                    return response;
                }

                // Check if email is being changed and already exists
                if (!string.IsNullOrEmpty(updateUserDTO.Email) && updateUserDTO.Email != existingUser.Email)
                {
                    var emailExists = await _context.UsersModel.FirstOrDefaultAsync(u => u.Email == updateUserDTO.Email);
                    if (emailExists != null)
                    {
                        response.Success = false;
                        response.Message = "User with this email already exists";
                        return response;
                    }
                }

                // Check if username is being changed and already exists
                if (!string.IsNullOrEmpty(updateUserDTO.Username) && updateUserDTO.Username != existingUser.Username)
                {
                    var usernameExists = await _context.UsersModel.FirstOrDefaultAsync(u => u.Username == updateUserDTO.Username);
                    if (usernameExists != null)
                    {
                        response.Success = false;
                        response.Message = "User with this username already exists";
                        return response;
                    }
                }

                // Update fields
                existingUser.Name = updateUserDTO.Name ?? existingUser.Name;
                existingUser.Email = updateUserDTO.Email ?? existingUser.Email;
                existingUser.Password = updateUserDTO.Password ?? existingUser.Password;
                existingUser.Username = updateUserDTO.Username ?? existingUser.Username;
                existingUser.Phone = updateUserDTO.Phone ?? existingUser.Phone;
                existingUser.role_id = updateUserDTO.role_id ?? existingUser.role_id;
                existingUser.role_name = updateUserDTO.role_name ?? existingUser.role_name;
                existingUser.isactive = updateUserDTO.isactive ?? existingUser.isactive;
                existingUser.isstaff = updateUserDTO.isstaff ?? existingUser.isstaff;
                existingUser.isadmin = updateUserDTO.isadmin ?? existingUser.isadmin;
                
                // Handle ImageUrl
                // If provided (not null), use it (can be empty string to remove image)
                // If null, preserve existing image
                if (updateUserDTO.ImageUrl != null)
                {
                    existingUser.ImageUrl = updateUserDTO.ImageUrl;
                }

                existingUser.ModifiedBy = "system"; // Or pass from DTO if available in common logic 
                existingUser.ModifiedOn = DateTime.UtcNow;

                _context.UsersModel.Update(existingUser);
                await _context.SaveChangesAsync();

                response.Success = true;
                response.Message = "User updated successfully";
                response.Data = existingUser;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error while updating user: {ex.Message}";
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
