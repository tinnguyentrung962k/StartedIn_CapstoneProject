using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface ITaskboardService
    {
        Task<Taskboard> CreateNewTaskboard(TaskboardCreateDTO taskboardCreateDto);
        Task<Taskboard> MoveTaskBoard(string taskBoardId, int position, bool needsReposition);
        Task<Taskboard> GetTaskboardById(string id);
        Task<Taskboard> UpdateTaskboard(string id, TaskboardInfoUpdateDTO taskboardInfoUpdateDTO);
    }
}
