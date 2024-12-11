using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class TerminationConfirmation : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(TerminationRequest))]
        public string TerminationRequestId { get; set; }
        public string ConfirmUserId { get; set; }
        public bool? IsAgreed { get; set; }
        public TerminationRequest TerminationRequest { get; set; }
    }
}
