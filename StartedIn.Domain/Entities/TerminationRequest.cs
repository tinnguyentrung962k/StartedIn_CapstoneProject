using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class TerminationRequest : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Contract))]
        public string ContractId { get; set; }
        public string FromId { get; set; }
        public string ToId { get; set; }
        public string Reason { get; set; }
        public bool? IsAgreed { get; set; }
        public Contract Contract { get; set; }
    }
}
