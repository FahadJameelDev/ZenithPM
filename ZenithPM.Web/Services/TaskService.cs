using Microsoft.EntityFrameworkCore;
using ZenithPM.Web.Data;
using ZenithPM.Web.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using TaskEntity = ZenithPM.Web.Models.Entities.Task;

namespace ZenithPM.Web.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task<List<TaskEntity>> GetAllTasksAsync(int organizationId)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .Include(t => t.AssignedBy)
                .Where(t => !t.IsDeleted && t.Project.OrganizationId == organizationId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task<TaskEntity?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .Include(t => t.AssignedBy)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }

        public async System.Threading.Tasks.Task<List<TaskEntity>> GetTasksByUserAsync(int userId)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Where(t => !t.IsDeleted && t.AssignedToId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task CreateTaskAsync(TaskEntity task)
        {
            task.CreatedAt = DateTime.UtcNow;
            task.IsDeleted = false;
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateTaskAsync(TaskEntity task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                task.IsDeleted = true;
                task.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}