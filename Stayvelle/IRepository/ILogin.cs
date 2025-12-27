using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.IRepository
{
    public interface ILogin
    {
        Task<Response<LoginResponseDTO>> LoginAsync(LoginRequestDTO loginRequest);
    }
}

