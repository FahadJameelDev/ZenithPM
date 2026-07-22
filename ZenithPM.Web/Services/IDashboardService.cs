using ZenithPM.Web.Models.ViewModels;

namespace ZenithPM.Web.Services
{
    public interface IDashboardService
    {
        Task<SuperAdminDashboardViewModel> GetSuperAdminDashboardAsync();
        Task<AdminDashboardViewModel> GetAdminDashboardAsync(int organizationId);
        Task<ManagerDashboardViewModel> GetManagerDashboardAsync(int userId);
        Task<EmployeeDashboardViewModel> GetEmployeeDashboardAsync(int userId);
    }
}