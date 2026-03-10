using CareerRookies.Web.ViewModels;

namespace CareerRookies.Web.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync();
}
