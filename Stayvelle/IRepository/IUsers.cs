using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.IRepository
{
    public interface IUsers
    {
        // Create
        Task<Response<UsersModel>> CreateUserAsync(CreateUserDTO createUserDTO);
        
        // Read
        Task<Response<List<UsersModel>>> GetAllUsersAsync();
        Task<Response<UsersModel?>> GetUserByIdAsync(int id);
        Task<Response<UsersModel?>> GetUserByEmailAsync(string email);
        Task<UsersModel?> GetUserByUsernameAsync(string username);
        Task<Response<List<UsersModel>>> GetHousekeepingUsersAsync();
        
        // Update
        Task<Response<UsersModel?>> UpdateUserAsync(int id, UpdateUserDTO updateUserDTO);
        
        // Delete
        Task<bool> DeleteUserAsync(int id);
        Task<bool> SoftDeleteUserAsync(int id); // Sets isdelete flag to true
    }
}
