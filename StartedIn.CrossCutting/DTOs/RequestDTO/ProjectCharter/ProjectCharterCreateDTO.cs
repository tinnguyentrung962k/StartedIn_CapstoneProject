﻿using StartedIn.CrossCutting.DTOs.RequestDTO.Milestone;
using StartedIn.CrossCutting.DTOs.RequestDTO.Phase;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.ProjectCharter
{
    public class ProjectCharterCreateDTO
    {
        public string? BusinessCase { get; set; }
        public string? Goal { get; set; }
        public string? Objective { get; set; }
        public string? Scope { get; set; }
        public string? Constraints { get; set; }
        public string? Assumptions { get; set; }
        public string? Deliverables { get; set; }
        public List<CreatePhaseDTO>? ListCreatePhaseDtos { get; set; }
    }
}
