using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class Finance : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Project))]
        public string ProjectId { get; set; }

        [Column(TypeName = "decimal(14,3)")]
        public decimal CurrentBudget { get; set; }

        [Column(TypeName = "decimal(14,3)")]
        public decimal TotalExpense { get; set; }

        [Column(TypeName = "decimal(14,3)")]
        public decimal RemainingDisbursement { get; set; }

        [Column(TypeName = "decimal(14,3)")]
        public decimal DisbursedAmount { get; set; }
        public Project Project { get; set; }
    }
}
