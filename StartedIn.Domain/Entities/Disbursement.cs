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
    public class Disbursement : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Contract))]
        public string? ContractId { get; set; }

        [ForeignKey(nameof(Investor))]
        public string InvestorId { get; set; }
        public string Title { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        
        [Column(TypeName = "decimal(14,3)")]
        public decimal Amount { get; set; }
        [MaxLength(500)]
        public string Condition { get; set; }
        public DisbursementStatusEnum DisbursementStatus { get; set; }
        
        [MaxLength(500)]
        public string? DeclineReason { get; set; }   
        public string? DealOfferId { get; set; }
        public long? OrderCode { get; set; }

        public bool IsValidWithContract { get; set; }
        public Contract? Contract { get; set; }
        public User Investor { get; set; }
        public ICollection<DisbursementAttachment>? DisbursementAttachments { get; set; }
        public Transaction Transaction { get; set; }
    }
}
