using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.IRepository
{
    public interface ILogin
    {
        Task<Response<LoginResponseDTO>> LoginAsync(LoginRequestDTO loginRequest);
    }
}

