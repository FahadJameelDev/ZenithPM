using Microsoft.EntityFrameworkCore;
using ZenithPM.Web.Data;
using ZenithPM.Web.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZenithPM.Web.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;

        public AttendanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Attendance>> GetAllAttendanceAsync(int organizationId)
        {
            return await _context.Attendances
                .Include(a => a.User)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<Attendance?> GetAttendanceByIdAsync(int id)
        {
            return await _context.Attendances
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Attendance?> GetAttendanceByDateAsync(int userId, DateTime date)
        {
            return await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date.Date == date.Date);
        }

        public async Task<List<Attendance>> GetAttendanceByUserAsync(int userId)
        {
            return await _context.Attendances
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public System.Threading.Tasks.Task CreateAttendanceAsync(Attendance attendance)
        {
            attendance.CreatedAt = DateTime.UtcNow;
            _context.Attendances.Add(attendance);
            return _context.SaveChangesAsync();
        }

        public System.Threading.Tasks.Task UpdateAttendanceAsync(Attendance attendance)
        {
            attendance.UpdatedAt = DateTime.UtcNow;
            _context.Attendances.Update(attendance);
            return _context.SaveChangesAsync();
        }
    }
}