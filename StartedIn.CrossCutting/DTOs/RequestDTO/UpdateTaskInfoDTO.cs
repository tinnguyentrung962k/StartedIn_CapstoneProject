﻿using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class UpdateTaskInfoDTO
    {
        public string TaskTitle { get; set; }
        public string Description { get; set; }
        public TaskEntityStatus Status { get; set; }
        public DateTimeOffset? Deadline { get; set; }
    }
}