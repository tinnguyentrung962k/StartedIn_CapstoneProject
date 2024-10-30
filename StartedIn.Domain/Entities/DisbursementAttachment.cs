using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class DisbursementAttachment : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Disbursement))]
        public string DisbursementId { get; set; }
        public string EvidenceFile { get; set; }
        public Disbursement Disbursement { get; set; }
    }
}
