using System.Collections.Generic;
using TaskEntity = ZenithPM.Web.Models.Entities.Task;

namespace ZenithPM.Web.Services
{
    public interface ITaskService
    {
        System.Threading.Tasks.Task<List<TaskEntity>> GetAllTasksAsync(int organizationId);
        System.Threading.Tasks.Task<TaskEntity?> GetTaskByIdAsync(int id);
        System.Threading.Tasks.Task<List<TaskEntity>> GetTasksByUserAsync(int userId);
        System.Threading.Tasks.Task CreateTaskAsync(TaskEntity task);
        System.Threading.Tasks.Task UpdateTaskAsync(TaskEntity task);
        System.Threading.Tasks.Task DeleteTaskAsync(int id);
    }
}