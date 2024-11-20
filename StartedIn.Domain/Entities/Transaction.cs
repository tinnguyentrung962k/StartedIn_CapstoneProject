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
    public class Transaction : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Finance))]
        public string FinanceId { get; set; }
        
        [ForeignKey(nameof(Disbursement))]
        public string? DisbursementId { get; set; }
        public string? AssetId { get; set; }

        [Column(TypeName = "decimal(14,3)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(14,3)")]
        public decimal? Budget { get; set; }
        public string? FromID { get; set; }
        public string? ToID { get; set; }
        public TransactionType Type { get; set; }
        public bool IsInFlow { get; set; }
        [MaxLength(500)]
        public string Content { get; set; }
        public string? EvidenceUrl { get; set; }
        public Disbursement? Disbursement { get; set; }
        public Finance Finance { get; set; }
        public Asset Asset { get; set; }

    }
}
