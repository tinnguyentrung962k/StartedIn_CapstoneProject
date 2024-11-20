﻿using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class Application : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Candidate))]
        public string CandidateId { get; set; }

        [ForeignKey(nameof(Recruitment))]
        public string RecruitmentId { get; set; }
        public ApplicationStatus Status { get; set; }
        public string CVUrl { get; set; }
        public User Candidate { get; set; }
        public Recruitment Recruitment { get; set; }
    }
}
