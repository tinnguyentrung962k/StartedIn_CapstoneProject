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
    public class LeavingRequest : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        
        [ForeignKey(nameof(Project))]
        public string ProjectId { get; set; }
        public LeavingRequestStatus Status { get; set; }
        public string? ConfirmUrl { get; set; }

        [MaxLength(500)]
        public string Reason { get; set; }
        public User User { get; set; }
        public Project Project { get; set; }
    }
}
