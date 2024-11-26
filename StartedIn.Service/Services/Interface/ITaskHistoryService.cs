using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface ITaskHistoryService
    {
        Task<IEnumerable<TaskHistory>> GetTaskHistoryOfTask(string projectId, string taskId, string userId);
    }
}
