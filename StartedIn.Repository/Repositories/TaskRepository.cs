﻿using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;

namespace StartedIn.Repository.Repositories
{
    public class TaskRepository : GenericRepository<TaskEntity, string>, ITaskRepository
    {
        public TaskRepository(AppDbContext context) : base(context)
        {
        }
    }
}
