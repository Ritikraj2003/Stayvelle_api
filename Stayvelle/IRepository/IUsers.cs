using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.IRepository
{
    public interface IUsers
    {
        // Create
        Task<Response<UsersModel>> CreateUserAsync(UsersModel user);
        
        // Read
        Task<Response<List<UsersModel>>> GetAllUsersAsync();
        Task<Response<UsersModel?>> GetUserByIdAsync(int id);
        Task<Response<UsersModel?>> GetUserByEmailAsync(string email);
        Task<UsersModel?> GetUserByUsernameAsync(string username);
        
        // Update
        Task<Response<UsersModel?>> UpdateUserAsync(int id, UsersModel user);
        
        // Delete
        Task<bool> DeleteUserAsync(int id);
        Task<bool> SoftDeleteUserAsync(int id); // Sets isdelete flag to true
    }
}
