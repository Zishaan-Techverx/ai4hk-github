using System.Security.Claims;
using System.Threading.Tasks;
using TpaSodManagement.ViewModels.Home;

namespace TpaSodManagement.Services.Interfaces
{
    public interface IHomeService
    {
        Task<HomeIndexViewModel> GetHomeIndexViewModelAsync(bool isAuthenticated);
    }
}

