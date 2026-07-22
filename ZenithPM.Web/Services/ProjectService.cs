using Microsoft.EntityFrameworkCore;
using ZenithPM.Web.Data;
using ZenithPM.Web.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZenithPM.Web.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;

        public ProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task<List<Project>> GetAllProjectsAsync(int organizationId)
        {
            return await _context.Projects
                .Include(p => p.Manager)
                .Include(p => p.Department)
                .Where(p => !p.IsDeleted && p.OrganizationId == organizationId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Manager)
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async System.Threading.Tasks.Task CreateProjectAsync(Project project)
        {
            project.CreatedAt = DateTime.UtcNow;
            project.IsDeleted = false;
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateProjectAsync(Project project)
        {
            project.UpdatedAt = DateTime.UtcNow;
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                project.IsDeleted = true;
                project.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async System.Threading.Tasks.Task<bool> ProjectExistsAsync(int id)
        {
            return await _context.Projects.AnyAsync(p => p.Id == id && !p.IsDeleted);
        }
    }
}