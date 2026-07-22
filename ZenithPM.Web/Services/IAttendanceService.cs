using System.Collections.Generic;
using System.Threading.Tasks;
using ZenithPM.Web.Models.Entities;

namespace ZenithPM.Web.Services
{
    public interface IAttendanceService
    {
        Task<List<Attendance>> GetAllAttendanceAsync(int organizationId);
        Task<Attendance?> GetAttendanceByIdAsync(int id);
        Task<Attendance?> GetAttendanceByDateAsync(int userId, System.DateTime date);
        Task<List<Attendance>> GetAttendanceByUserAsync(int userId);
        System.Threading.Tasks.Task CreateAttendanceAsync(Attendance attendance);
        System.Threading.Tasks.Task UpdateAttendanceAsync(Attendance attendance);
    }
}