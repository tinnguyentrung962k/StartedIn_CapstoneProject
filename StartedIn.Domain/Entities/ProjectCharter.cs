using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class ProjectCharter : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Project))]
        public string ProjectId { get; set; }
        
        [MaxLength(255)]
        public string? BusinessCase { get; set; }
        [MaxLength(255)]
        public string? Goal { get; set; }

        [MaxLength(255)]
        public string? Objective { get; set; }
        
        [MaxLength(255)]
        public string? Scope { get; set; }
        
        [MaxLength(255)]
        public string? Constraints { get; set; }
        
        [MaxLength(255)]
        public string? Assumptions { get; set; }
        [MaxLength(255)]
        public string? Deliverables { get; set; }

        public Project Project { get; set; }
        public ICollection<Milestone>? Milestones { get; set; }
    }
}
