﻿using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class UserTask
    {
        public string UserId { get; set; }
        public string TaskId { get; set; }
        public virtual User User { get; set; }
        public virtual TaskEntity Task { get; set; }
        public TaskAssignType TaskAssignType { get; set; }

    }
}