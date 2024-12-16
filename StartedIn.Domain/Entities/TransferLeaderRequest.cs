using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class TransferLeaderRequest : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Project))]
        public string ProjectId { get; set; }
        public string FormerLeaderId { get; set; }
        public string NewLeaderId { get; set; }
        public DateOnly TransferDate { get; set; }
        public bool? IsAgreed { get; set; }
        public string AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
        public Project Project { get; set; }
    }
}
