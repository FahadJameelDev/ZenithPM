using ZenithPM.Web.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZenithPM.Web.Services
{
    public interface IProjectService
    {
        System.Threading.Tasks.Task<List<Project>> GetAllProjectsAsync(int organizationId);
        System.Threading.Tasks.Task<Project?> GetProjectByIdAsync(int id);
        System.Threading.Tasks.Task CreateProjectAsync(Project project);
        System.Threading.Tasks.Task UpdateProjectAsync(Project project);
        System.Threading.Tasks.Task DeleteProjectAsync(int id);
        System.Threading.Tasks.Task<bool> ProjectExistsAsync(int id);
    }
}